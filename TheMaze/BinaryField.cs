using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace TheMaze
{
    public class BinaryField
    {
        //FormMaze parentForm;

        List<Room> rooms; // список всех комнат 
        List<List<Room>> roomUnion; // список объединенных естественным путем (налоежнием друг на друга) комнат
        public bool[,] Field { get; private set; } // поле
        public int Height { get; private set; } = 23;
        public int Width { get; private set; } = 41;
        // Label res = new Label();
        Random r;
        struct Room
        {
            internal readonly Point upLeft;
            internal readonly Point downRight;
            internal Room(Point upLeft, Point downRight)
            {
                this.upLeft = upLeft;
                this.downRight = downRight;
            }
        }
        public BinaryField(/*FormMaze parentForm*/)
        {
            //this.parentForm = parentForm;
            rooms = new List<Room>();
            roomUnion = new List<List<Room>>();
            Field = new bool[Height, Width];
            r = new Random();
            // res.AutoSize = true;
            // res.Parent = parentForm;
            // parentForm.KeyDown += FormMaze_KeyDown;
        }

        public void GenerateMaze()
        {
            // вся карта делится на 4 сектора + центр, каждый из которых случайно генерируется
            GenerateSector(new Point(1, 1), new Point(Width / 2, Height / 2)); // левый верхний
            GenerateSector(new Point(1, Height / 2), new Point(Width / 2, Height - 1)); // левый нижний
            GenerateSector(new Point(Width / 2, 1), new Point(Width, Height / 2)); // правый верхний    
            GenerateSector(new Point(Width / 2, Height / 2), new Point(Width, Height - 1)); // правый нижний
            GenerateSector(new Point(Width / 4, Height / 4), new Point((Width / 3) * 2, (Height / 3) * 2), true); // центр
            ChekcSingleRoom(); // создаём объединение для комнат, которые не вошли ни в одно объединение (необходимо для построения мостов)
            //res.Text = "";
            //StringBuilder resb = new StringBuilder();
            //for (int i = 0; i < Height; i++)
            //{
            //    for (int k = 0; k < Width; k++)
            //    {
            //        if (Field[i, k] == true)
            //            resb.Append('1');
            //        else
            //            resb.Append('0');
            //    }
            //    resb.Append('\n');
            //}
            //res.Text = resb.ToString();
            //resb = null;

            GenerateBridges(); // создаём мосты между комнатами
        }

        private void ChekcSingleRoom()
        {
            foreach (Room item in rooms)
            {
                if (!roomUnion.Contains(roomUnion.Find(cur => cur.Contains(item))))
                {
                    List<Room> newUnion = new List<Room>();
                    newUnion.Add(item);
                    roomUnion.Add(newUnion);
                }
            }
        }

        void GenerateSector(Point LocationFrom, Point LocationTo, bool isCenter = false)
        {
            // если это любой другой кроме центра сектор, генерируем случайное количество комнат, иначе одну
            int roomCount = 0;
            if (!isCenter)
                roomCount = r.Next(2, 5);
            else
                roomCount = 1;

            for (int i = 0; i < roomCount; i++)
            {
                int xKoef = Width / 4;
                int yKoef = Height / 4;
                int xKoef2 = Width / 6;
                int yKoef2 = Height / 6 - 1;
                // создание одной комнаты
                Point UpLeft = new Point(
                    r.Next(LocationFrom.X, LocationTo.X - xKoef),
                    r.Next(LocationFrom.Y, LocationTo.Y - yKoef));
                Point downRight = new Point(
                    r.Next(UpLeft.X + xKoef2, UpLeft.X + xKoef - 1 > LocationTo.X ? LocationTo.X - UpLeft.X : UpLeft.X + xKoef - 1),
                    r.Next(UpLeft.Y + yKoef2, UpLeft.Y + yKoef - 2 > LocationTo.Y ? LocationTo.Y - UpLeft.Y : UpLeft.Y + yKoef - 2));

                Room current = new Room(UpLeft, downRight);
                FillRoomBounds(current); // заполняем границы комнаты
                CheckCollision(current); // проверяем наличие коллизий
                rooms.Add(current);
            }
        }

        private void FillRoomBounds(Room room)
        {
            for (int i = room.upLeft.Y; i <= room.downRight.Y; i++)
                for (int k = room.upLeft.X; k <= room.downRight.X; k++)
                    Field[i, k] = true;
        }

        private void CheckCollision(Room forRoom)
        {
            foreach (Room item in rooms)
            {
                // ищем пересечение комнат по отношению к item
                if ((item.upLeft.X <= forRoom.upLeft.X // справа вверху
                    && item.downRight.X >= forRoom.upLeft.X
                    && item.upLeft.Y <= forRoom.downRight.Y
                    && item.downRight.Y >= forRoom.downRight.Y)
                    ||
                    (item.upLeft.X <= forRoom.upLeft.X // справа внизу
                    && item.downRight.X >= forRoom.upLeft.X
                    && item.upLeft.Y <= forRoom.upLeft.Y
                    && item.downRight.Y >= forRoom.upLeft.Y)
                    ||
                    (item.upLeft.X <= forRoom.downRight.X // слева вверху
                    && item.downRight.X >= forRoom.downRight.X
                    && item.upLeft.Y <= forRoom.downRight.Y
                    && item.downRight.Y >= forRoom.downRight.Y)
                    ||
                    (item.upLeft.X <= forRoom.downRight.X // слева внизу
                    && item.downRight.X >= forRoom.downRight.X
                    && item.upLeft.Y <= forRoom.upLeft.Y
                    && item.downRight.Y >= forRoom.upLeft.Y))
                {
                    // ищем не содержится ли item или forRoom в существующих коллекциях объединений,
                    // и: 1) либо добавляем чего не хватает,
                    //    2) либо создаём такую коллекцию, если ни того ни другого не найдно
                    //    3) либо не добавляем ничего, если оба элемента уже находятся в ОДНОМ объединении

                    // item и forRoom могут содержаться в разных объдинениях, поэтому нужно отдельно 
                    // провести поиск обоих элементов в рамках одной коллекции
                    bool itemAndForRoomFound =
                        roomUnion.Contains(
                            roomUnion.Find(curRoomList => curRoomList.Contains(item) && curRoomList.Contains(forRoom)));

                    if (itemAndForRoomFound)
                        continue;

                    List<Room> itemFound = roomUnion.Find(curRoomList => curRoomList.Contains(item));
                    List<Room> forRoomFound = roomUnion.Find(curRoomList => curRoomList.Contains(forRoom));

                    // комната объединяет объединения
                    if (forRoomFound != null && itemFound != null)
                    {
                        List<Room> newUinon = new List<Room>();
                        newUinon.AddRange(itemFound);
                        newUinon.AddRange(forRoomFound);
                        roomUnion.Remove(itemFound);
                        roomUnion.Remove(forRoomFound);
                        roomUnion.Add(newUinon);
                    }

                    if (forRoomFound != null)
                        forRoomFound.Add(item);

                    else if (itemFound != null)
                        itemFound.Add(forRoom);

                    else
                    {
                        List<Room> newUnion = new List<Room>();
                        newUnion.Add(item);
                        newUnion.Add(forRoom);
                        roomUnion.Add(newUnion);
                    }
                }
            }
        }

        private void GenerateBridges()
        {
            // определяем крайние границы объединения
            List<Point[]> connectors = DefineConnectors();
            foreach (Point[] item in connectors)
            {
                // 0 - лево, 1 - право, 2 - верх, 3 - низ
                // проверяем существует ли путь от крайней стены в соответствующую сторону 
                // если соединение угловое, определяем его конечную точку в методах типа Exist
                Point endPoint;
                if (LeftDownBridgeExist(item[0], out endPoint))
                    LeftDownBridgeCreate(item[0], endPoint);

                if (LeftBridgeExist(item[0]))
                    LeftBridgeCreate(item[0]);

                if (LeftUpBridgeExist(item[0], out endPoint))
                    LeftUpBridgeCreate(item[0], endPoint);

                if (RightDownBridgeExist(item[1], out endPoint))
                    RightDownBridgeCreate(item[1], endPoint);

                if (RightBridgeExist(item[1]))
                    RightBridgeCreate(item[1]);

                if (RightUpBridgeExist(item[1], out endPoint))
                    RightUpBridgeCreate(item[1], endPoint);

                if (UpLeftBridgeExist(item[2], out endPoint))
                    UpLeftBridgeCreate(item[2], endPoint);

                if (UpBridgeExist(item[2]))
                    UpBridgeCreate(item[2]);

                if (UpRightBridgeExist(item[2], out endPoint))
                    UpRightBridgeCreate(item[2], endPoint);

                if (DownLeftBridgeExist(item[3], out endPoint))
                    DownLeftBridgeCreate(item[3], endPoint);

                if (DownBridgeExist(item[3]))
                    DownBridgeCreate(item[3]);

                if (DownRightBridgeExist(item[3], out endPoint))
                    DownRightBridgeCreate(item[3], endPoint);
            }
        }

        private List<Point[]> DefineConnectors()
        {
            // массив состоит из четырех точек: левая, правая, верхняя и нижняя границы
            // создаём список таких массивов для каждого объединения соответственно
            List<Point[]> res = new List<Point[]>();
            foreach (List<Room> item in roomUnion)
            {
                // 0 - лево, 1 - право, 2 - верх, 3 - низ
                Point[] bounds = new Point[4];
                Room left = item.Find(cur => item.Min(room => room.upLeft.X) == cur.upLeft.X);
                Room right = item.Find(cur => item.Max(room => room.downRight.X) == cur.downRight.X);
                Room up = item.Find(cur => item.Min(room => room.upLeft.Y) == cur.upLeft.Y);
                Room down = item.Find(cur => item.Max(room => room.downRight.Y) == cur.downRight.Y);
                bounds[0] = new Point(left.upLeft.X, (left.upLeft.Y + left.downRight.Y) / 2);
                bounds[1] = new Point(right.downRight.X, (right.upLeft.Y + right.downRight.Y) / 2);
                bounds[2] = new Point((up.upLeft.X + up.downRight.X) / 2, up.upLeft.Y);
                bounds[3] = new Point((down.upLeft.X + down.downRight.X) / 2, down.downRight.Y);
                res.Add(bounds);
            }
            return res;
        }

        private void DownRightBridgeCreate(Point downBound, Point endPoint)
        {
            for (int i = downBound.Y + 1; i <= endPoint.Y; i++)
            {
                Field[i, downBound.X] = true;
                Field[i, downBound.X + 1] = true;
            }
            for (int i = downBound.X; i <= endPoint.X; i++)
            {
                Field[endPoint.Y, i] = true;
                Field[endPoint.Y + 1, i] = true;
            }
        }

        private bool DownRightBridgeExist(Point downBound, out Point endPoint)
        {
            for (int i = downBound.Y + 2; i < Height; i++)
            {
                for (int k = downBound.X; k < Width; k++)
                {
                    if (Field[i, k] == false || Field[i + 1, k] == false)
                        continue;

                    endPoint = new Point(k, i);
                    return true;
                }
            }
            endPoint = default;
            return false;
        }

        private void DownBridgeCreate(Point downBound)
        {
            for (int i = downBound.Y + 1; i < Height; i++)
            {
                if (Field[i, downBound.X] == true && Field[i, downBound.X + 1] == true)
                    break;

                Field[i, downBound.X] = true;
                Field[i, downBound.X + 1] = true;
            }
        }

        private bool DownBridgeExist(Point downBound)
        {
            for (int i = downBound.Y + 1; i < Height; i++)
            {
                if (Field[i, downBound.X] == false || Field[i, downBound.X + 1] == false)
                    continue;
                return true;
            }
            return false;
        }

        private void DownLeftBridgeCreate(Point downBound, Point endPoint)
        {
            for (int i = downBound.Y + 1; i <= endPoint.Y; i++)
            {
                Field[i, downBound.X] = true;
                Field[i, downBound.X + 1] = true;
            }
            for (int i = downBound.X; i >= endPoint.X; i--)
            {
                Field[endPoint.Y, i] = true;
                Field[endPoint.Y + 1, i] = true;
            }
        }

        private bool DownLeftBridgeExist(Point downBound, out Point endPoint)
        {
            for (int i = downBound.Y + 2; i < Height; i++)
            {
                for (int k = downBound.X; k > 0; k--)
                {
                    if (Field[i, k] == false || Field[i + 1, k] == false)
                        continue;

                    endPoint = new Point(k, i);
                    return true;
                }
            }
            endPoint = default;
            return false;
        }

        private void UpRightBridgeCreate(Point upBound, Point endPoint)
        {
            for (int i = upBound.Y - 1; i >= endPoint.Y; i--)
            {
                Field[i, upBound.X] = true;
                Field[i, upBound.X + 1] = true;
            }
            for (int i = upBound.X; i <= endPoint.X; i++)
            {
                Field[endPoint.Y, i] = true;
                Field[endPoint.Y + 1, i] = true;
            }
        }

        private bool UpRightBridgeExist(Point upBound, out Point endPoint)
        {
            for (int i = upBound.Y - 2; i > 0; i--)
            {
                for (int k = upBound.X; k < Width; k++)
                {
                    if (Field[i, k] == false || Field[i + 1, k] == false)
                        continue;

                    endPoint = new Point(k, i);
                    return true;
                }
            }
            endPoint = default;
            return false;
        }

        private void UpBridgeCreate(Point upBound)
        {
            for (int i = upBound.Y - 1; i > 0; i--)
            {
                if (Field[i, upBound.X] == true && Field[i, upBound.X + 1] == true)
                    break;

                Field[i, upBound.X] = true;
                Field[i, upBound.X + 1] = true;
            }
        }

        private bool UpBridgeExist(Point upBound)
        {
            for (int i = upBound.Y - 1; i > 0; i--)
            {
                if (Field[i, upBound.X] == false || Field[i, upBound.X + 1] == false)
                    continue;
                return true;
            }
            return false;
        }

        private void UpLeftBridgeCreate(Point upBound, Point endPoint)
        {
            for (int i = upBound.Y - 1; i >= endPoint.Y; i--)
            {
                Field[i, upBound.X] = true;
                Field[i, upBound.X + 1] = true;
            }
            for (int i = upBound.X; i >= endPoint.X; i--)
            {
                Field[endPoint.Y, i] = true;
                Field[endPoint.Y + 1, i] = true;
            }
        }

        private bool UpLeftBridgeExist(Point upBound, out Point endPoint)
        {
            for (int i = upBound.Y - 2; i > 0; i--)
            {
                for (int k = upBound.X; k > 0; k--)
                {
                    if (Field[i, k] == false || Field[i + 1, k] == false)
                        continue;

                    endPoint = new Point(k, i);
                    return true;
                }
            }
            endPoint = default;
            return false;
        }

        private void RightUpBridgeCreate(Point rightBound, Point endPoint)
        {
            for (int i = rightBound.X + 1; i <= endPoint.X; i++)
            {
                Field[rightBound.Y, i] = true;
                Field[rightBound.Y + 1, i] = true;
            }
            for (int i = rightBound.Y; i >= endPoint.Y; i--)
            {
                Field[i, endPoint.X] = true;
                Field[i, endPoint.X + 1] = true;
            }
        }

        private bool RightUpBridgeExist(Point rightBound, out Point endPoint)
        {
            for (int i = rightBound.X + 2; i < Width; i++)
            {
                for (int k = rightBound.Y; k > 0; k--)
                {
                    if (i + 1 > Width - 1)
                        break;

                    if (Field[k, i] == false || Field[k, i + 1] == false)
                        continue;

                    endPoint = new Point(i, k);
                    return true;
                }
            }
            endPoint = default;
            return false;
        }

        private void RightBridgeCreate(Point rightBound)
        {
            for (int i = rightBound.X + 1; i < Width; i++)
            {
                if (Field[rightBound.Y, i] == true && Field[rightBound.Y + 1, i] == true)
                    break;

                Field[rightBound.Y, i] = true;
                Field[rightBound.Y + 1, i] = true;
            }
        }

        private bool RightBridgeExist(Point rightBound)
        {
            for (int i = rightBound.X + 1; i < Width; i++)
            {
                if (Field[rightBound.Y, i] == false || Field[rightBound.Y + 1, i] == false)
                    continue;
                return true;
            }
            return false;
        }

        private void RightDownBridgeCreate(Point rightBound, Point endPoint)
        {
            for (int i = rightBound.X + 1; i <= endPoint.X; i++)
            {
                Field[rightBound.Y, i] = true;
                Field[rightBound.Y + 1, i] = true;
            }
            for (int i = rightBound.Y; i <= endPoint.Y; i++)
            {
                Field[i, endPoint.X] = true;
                Field[i, endPoint.X + 1] = true;
            }
        }

        private bool RightDownBridgeExist(Point rightBound, out Point endPoint)
        {
            for (int i = rightBound.X + 2; i < Width; i++)
            {
                for (int k = rightBound.Y; k < Height; k++)
                {
                    if (i + 1 > Width - 1)
                        break;

                    if (Field[k, i] == false || Field[k, i + 1] == false)
                        continue;

                    endPoint = new Point(i, k);
                    return true;
                }
            }
            endPoint = default;
            return false;
        }

        private void LeftUpBridgeCreate(Point leftBound, Point endPoint)
        {
            for (int i = leftBound.X - 1; i >= endPoint.X; i--)
            {
                Field[leftBound.Y, i] = true;
                Field[leftBound.Y + 1, i] = true;
            }
            for (int i = leftBound.Y; i >= endPoint.Y; i--)
            {
                Field[i, endPoint.X] = true;
                Field[i, endPoint.X - 1] = true;
            }
        }

        private bool LeftUpBridgeExist(Point leftBound, out Point endPoint)
        {
            for (int i = leftBound.X - 2; i > 0; i--)
            {
                for (int k = leftBound.Y; k > 0; k--)
                {
                    if (i - 1 < 0)
                        break;

                    if (Field[k, i] == false || Field[k, i - 1] == false)
                        continue;

                    endPoint = new Point(i, k);
                    return true;
                }
            }
            endPoint = default;
            return false;
        }

        private void LeftBridgeCreate(Point leftBound)
        {
            for (int i = leftBound.X - 1; i > 0; i--)
            {
                if (Field[leftBound.Y, i] == true && Field[leftBound.Y + 1, i] == true)
                    break;

                Field[leftBound.Y, i] = true;
                Field[leftBound.Y + 1, i] = true;
            }
        }

        private bool LeftBridgeExist(Point leftBound)
        {
            for (int i = leftBound.X - 1; i > 0; i--)
            {
                if (Field[leftBound.Y, i] == false || Field[leftBound.Y + 1, i] == false)
                    continue;
                return true;
            }
            return false;
        }

        private void LeftDownBridgeCreate(Point leftBound, Point endPoint)
        {
            for (int i = leftBound.X - 1; i >= endPoint.X; i--)
            {
                Field[leftBound.Y, i] = true;
                Field[leftBound.Y + 1, i] = true;
            }
            for (int i = leftBound.Y; i <= endPoint.Y; i++)
            {
                Field[i, endPoint.X] = true;
                Field[i, endPoint.X - 1] = true;
            }
        }

        private bool LeftDownBridgeExist(Point leftBound, out Point endPoint)
        {
            for (int i = leftBound.X - 2; i > 0; i--)
            {
                for (int k = leftBound.Y; k < Height - 1; k++)
                {
                    if (i - 1 < 0)
                        break;

                    if (Field[k, i] == false || Field[k, i - 1] == false)
                        continue;

                    endPoint = new Point(i, k);
                    return true;
                }
            }
            endPoint = default;
            return false;
        }

        // private void FormMaze_KeyDown(object sender, KeyEventArgs e)
        // {
        //     res.Text = "";
        //     StringBuilder resb = new StringBuilder();
        //     for (int i = 0; i < Height; i++)
        //     {
        //         for (int k = 0; k < Width; k++)
        //         {
        //             if (Field[i, k] == true)
        //                 resb.Append('1');
        //             else
        //                 resb.Append('0');
        //         }
        //         resb.Append('\n');
        //     }
        //     res.Text = resb.ToString();
        //     resb = null;
        // }
    }
}