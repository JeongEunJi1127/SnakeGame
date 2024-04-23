using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Xml.Linq;

namespace SnakeGame
{
    class Program
    {
        static void Main(string[] args)
        {
            Queue<Point> snakeQueue = new Queue<Point>();
            Queue<Point> foodQueue = new Queue<Point>();

            // 맵 위치 설정
            int width = 80;
            int height = 20;

            int eatenFood = 0;

            // 맵 그리기
            Console.WriteLine(new string('-', width));
            for (int i = 0; i < height; i++)
            {
                Console.WriteLine("|" + new string(' ', width) + "|");
            }
            Console.WriteLine(new string('-', width));

            Console.WriteLine("스네이크 게임을 시작합니다.");
            Console.WriteLine("방향키를 눌러 뱀을 움직일 수 있습니다");
            Console.WriteLine("W - 위, A - 왼쪽, S - 아래, D - 오른쪽\n");
            
            // 뱀의 초기 위치와 방향을 설정
            Point p = new Point(4, 5, '*');
            Snake snake = new Snake(p, 4, Direction.RIGHT);
            snakeQueue.Enqueue(p);
            snake.Draw(snakeQueue);

            // 음식의 위치를 무작위로 생성
            FoodCreator foodCreator = new FoodCreator(width, height, '$');
            Point food = foodCreator.CreateFood(snakeQueue);
            foodQueue.Enqueue(food);
            food.Draw(foodQueue);

            while (true)
            {
                Console.SetCursorPosition(0, height + 6); // 보드 바로 아래로 커서 이동
                Console.WriteLine("현재 뱀의 길이 : " + snake.Length);
                Console.WriteLine("현재 뱀이 먹은 음식 수 : " + eatenFood);
                snake.Move(snakeQueue);

                if (snake.CollideToFood(food,foodQueue))
                {
                    Console.SetCursorPosition(0, height + 9);
                    Console.WriteLine("음식을 먹었습니다! " );
                    Console.SetCursorPosition(0, height + 9);
                    Thread.Sleep(300);
                    Console.Write("                                         ");

                    eatenFood++;
                    snake.Length ++;

                    food = foodCreator.CreateFood(snakeQueue);
                    foodQueue.Enqueue(food);
                    food.Draw(foodQueue);
                }

                // 뱀이 벽, 자신의 몸에 충돌
                if (snake.CollideToWall(width,height))
                {
                    Console.SetCursorPosition(0, height + 9);
                    Console.WriteLine("저런.. 벽에 부딫쳤어요!");
                    Console.WriteLine("GAME OVER");
                    break;
                }
                if ( snake.CollideToSnake(snakeQueue))
                {
                    Console.SetCursorPosition(0, height + 9);
                    Console.WriteLine("나랑 부딫쳤어!");
                    Console.WriteLine("GAME OVER");
                    break;
                }
                Thread.Sleep(50); // 게임 속도 조절
            }
        }
    }

    //  뱀의 상태와 이동, 음식 먹기, 자신의 몸에 부딪혔는지 확인 기능
    public class Snake
    {
        public Point P { get; set; }
        public int Length { get; set; }
        public Direction Dir { get; set; }
        // Snake 클래스 생성자
        public Snake(Point point, int length,Direction dir)
        {
            P = point;
            Length = length;
            Dir = dir;
        }
        // Snake 위치 그리는 메서드
        public void Draw(Queue<Point> queue)
        {
            foreach(Point p in queue)
            {
                Console.SetCursorPosition(p.x, p.y);
                Console.Write(p.sym);
            }
        }
        // Snake 이동 메서드
        public void Move(Queue<Point> queue)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            Point newP = new Point(P.x, P.y, P.sym);

            if (keyInfo.Key == ConsoleKey.W)
            {
                Dir = Direction.UP;
                newP.y--;
            }
            else if (keyInfo.Key == ConsoleKey.S)
            {
                Dir = Direction.DOWN;
                newP.y++;
            }
            else if (keyInfo.Key == ConsoleKey.A)
            {
                Dir = Direction.LEFT;
                newP.x--;
            }
            else if (keyInfo.Key == ConsoleKey.D)
            {
                Dir = Direction.RIGHT;
                newP.x++;
            }
            P = newP;
            queue.Enqueue(P);

            if (queue.Count > Length)
            {
                Point deleteP = queue.Dequeue();
                deleteP.Clear();
            }

            Draw(queue);
        }
        // Snake와 음식 충돌 메서드
        public bool CollideToFood(Point point, Queue<Point> queue)
        {
            if (P.IsHit(point))
            {
                queue.Dequeue();
                point.sym = '*';
                return true;
            }
            return false;
        }
        // Snake와 벽 충돌 메서드
        public bool CollideToWall(int width, int height)
        {
            if (P.x < 1 || P.x >= width+1 || P.y < 1  || P.y >=height+1)
                return true;
            return false;
         }
        // Snake와 자기자신 충돌 메서드
        public bool CollideToSnake(Queue<Point> queue)
        {
            foreach (Point p in queue)
            {
                if (p != P && P.IsHit(p))
                {
                    return true;
                }
            }
            return false;
        }
    }
    // 맵의 크기 내에서 무작위 위치에 음식을 생성
    public class FoodCreator
    {
        public Point point { get; set; }

        Random rand = new Random();
        public FoodCreator(int x, int y, char z)
        {
            point = new Point(x, y, z);
        }
        // 랜덤 위치에 음식 생성 메서드 - 뱀이 있는 위치에는 생성 x
        public Point CreateFood(Queue<Point> queue)
        {
            while (true)
            {
                Point newP = new Point(rand.Next(1, point.x + 1), rand.Next(1, point.y + 1), point.sym);
                foreach (Point p in queue)
                {
                    if (p != newP) { return newP;}
                }
            }
        }
    }

    public class Point
    {
        public int x { get; set; }
        public int y { get; set; }
        public char sym { get; set; }

        // Point 클래스 생성자
        public Point(int _x, int _y, char _sym)
        {
            x = _x;
            y = _y;
            sym = _sym;
        }

        // 점을 그리는 메서드
        public void Draw(Queue<Point> queue)
        {
            foreach (Point p in queue)
            {
                Console.SetCursorPosition(p.x, p.y);
                Console.Write(p.sym);
            }
        }
        // 점을 지우는 메서드
        public void Clear()
        {
            Console.SetCursorPosition(x, y);
            Console.Write(" ");
        }

        // 두 점이 같은지 비교하는 메서드
        public bool IsHit(Point p)
        {
            return p.x == x && p.y == y;
        }
    }
    // 방향을 표현하는 열거형입니다.
    public enum Direction
    {
        LEFT,
        RIGHT,
        UP,
        DOWN
    }
}
