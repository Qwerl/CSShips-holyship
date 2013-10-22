using System;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
    
//абстрактный базовый класс кораблей
class Ship
    {
      private int speed;                //скорость корабля 
      private int num;                  //номер корабля-количество кораблей
      protected int x, y;               //координаты корабля

      int constsleep;                   //скорость потока
      private int xPort, yPort;         //координаты цели(порта)
      protected bool life;              //признак жизни потока
      protected Thread thr;             //ссылка на поток    
      //свойства корабля
      public int N { get { return num; } }
      public int X { get { return x; } }
      public int Y { get { return y; } }

      public int Speed
      {
         get {return speed;}
         set {speed=value;}
      }

      public Ship(int N, int X, int Y, int Speed)
      {
        num = N; x = X; y = Y; speed = Speed;
        //Создать ,но не запускать
        life = true;
        thr = new Thread(new ThreadStart(Move));
        //thr.Start();
      }

      //абстрактная функция передвижения кораблей
      public void Move()
      { 
        int dx,dy;
        while(life)
        {
          dx = xPort - x;
          dy = yPort - y;
        }
      }

      //Завершить поток
      public void Finish() { life = false; }
    }

//базовый класс причалов
class Port //отсюда породится 3 порта
    {
        protected int x, y;                 //координаты порта
        private bool free = true;           //индикатор свободности
        private int sleeptime;              //время простаивания в порте
        //свойства причала
        public int Sleeptime
        {
            get { return sleeptime; }
            set { sleeptime = value; }
        }
    }


//класс данных событий кораблей (разобраться зачем он)
class Data : EventArgs
{  
  private int n,x,y;
  public int N {get {return n;}}
  public int X {get {return x;}}
  public int Y {get {return y;}}

  public Data(int N, int X, int Y)
  { n = N; x = X; y = Y; }
}



//Делегат события(для чего он?)
delegate void DelEv(Data d);






//окошко
class Window : Form
{ 
  Ship  ship1;

  public Window ()
  {
      ship1 = new Ship(1, 2, 150, 20);
      //ship1.evShip += new DelShip(this.HandlerShip);
  }

  private void HandlerShip(Data D)
  {
      Invalidate();//перерисовать
  }

  protected override void OnPaint(PaintEventArgs e)
  {
      
      base.OnPaint(e);
      e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(255,0,0)) , ship1.X, ship1.Y, 50, 20);
  }
}


class Program
    {
        static void Main(string[] args)
            {
                Application.Run(new Window());
            }
    }
