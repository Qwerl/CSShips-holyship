using System;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
    
//абстрактный базовый класс кораблей
class Ship
    {
      private int speed;                //скорость корабл€ 
      private int num;                  //номер корабл€-количество кораблей
      protected int x, y;               //координаты корабл€
      int constsleep=50;                   //скорость потока
      private int xPort, yPort;         //координаты цели(порта)
      protected bool life;              //признак жизни потока
      protected Thread thr;             //ссылка на поток    
      //свойства корабл€
      public int N { get { return num; } }
      public int X { get { return x; } }
      public int Y { get { return y; } }

      public int Speed
      {
         get {return speed;}
         set {speed=value;}
      }
      
      Window w;
      public event DelShip evShip;

      public Ship(int N, int X, int Y, int Speed,Window W)//конструктор вроде называетс€
      {
          num = N; x = X; y = Y; speed = Speed; w = W;
        //—оздать поток
        life = true;
        thr = new Thread(new ThreadStart(Move));
        thr.Start();
      }

      //абстрактна€ функци€ передвижени€ кораблей
      public void Move()
      { 
        int dx=1,dy=1;
        while(life)
        { 
          System.Console.WriteLine("{0}",xPort);
          dx = xPort - x;
          dy = yPort - y;
          x += dx / 5;
          y += dy / 5;
         // x += dx;
          if ((x < 0 || x > w.ClientSize.Width) && (y < 0 || y > w.ClientSize.Height)) //отталкивание от стенок (пока забиваетс€ в угол)
          { dx = -dx; dy = -dy; }
            Data d = new Data(N, X, Y);
            if (evShip != null)
                evShip(d);
            Thread.Sleep(constsleep);
        }
      }
      public void HandlerEv(Data D)
      {
          Data d = (Data)D;
          this.xPort=d.X; this.yPort=d.Y;
      }

      //«авершить поток
      public void Finish() { life = false; }



    }

//базовый класс причалов
class Port //отсюда породитс€ 3 порта
    {
    private int num;
        protected int x, y;                 //координаты порта
        private bool free;                  //индикатор свободности
        private int sleeptime;              //врем€ простаивани€ в порте
        //свойства причала
        public int Sleeptime
        {
            get { return sleeptime; }
            set { sleeptime = value; }

        }
        public int N { get { return num; } }
        public int X { get { return x; } }
        public int Y { get { return y; } }

        Window w;
        public event DelShip evShip;

        public Port(int N, int X, int Y,int Sleeptime, Window W )
        {
            num = N; x = X; y = Y; sleeptime = Sleeptime; w = W;
        }
    }


//класс данных событий кораблей (разобратьс€ зачем он)
class Data : EventArgs
{  
  private int n,x,y;
  public int N {get {return n;}}
  public int X {get {return x;}}
  public int Y {get {return y;}}

  public Data(int N, int X, int Y)
  { n = N; x = X; y = Y; }
}



//ƒелегат событи€(дл€ чего он?)
delegate void DelShip(Data d);






//окошко
class Window : Form
{ 
  Ship  ship1,ship2;
    Port port1, port2, port3;
  Font aFont = new Font("Tahoma", 12, FontStyle.Regular);

  public Window ()
  {
      port1 = new Port(1, 50, 100,200,this);
      port1.evShip += new DelShip(this.HandlerShip);
      port2 = new Port(1, 200, 100,200,this);
     // port2.evShip += new DelShip(this.HandlerShip);
      port3 = new Port(1, 50, 300,200,this);
      //port3.evShip += new DelShip(this.HandlerShip);
      ship1 = new Ship(1, 100, 100, 20, this);//создать объект корабл€ с параметрами : первый параметр номер корабл€ , второй ’ ,третий ” , четвертый скорость
      ship1.evShip += new DelShip(this.HandlerShip);
      ship2 = new Ship(2, 300, 300, 20, this);//создать объект корабл€ с параметрами : первый параметр номер корабл€ , второй ’ ,третий ” , четвертый скорость
      ship2.evShip += new DelShip(this.HandlerShip);
  }

  private void HandlerShip(Data D)
  {
      Invalidate();//перерисовать
  }

  protected override void OnPaint(PaintEventArgs e)
  {
      
      base.OnPaint(e);
      e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(255, 0, 0)), ship1.X, ship1.Y, 50, 20); //нарисовать элипс с цветом красным  , координатами корабл€1 шириной 50 высотой 20
      e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(0, 255, 0)), port1.X, port1.Y, 20, 20);
      e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(0, 0, 250)), port2.X, port2.Y, 20, 20); 
      e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(255, 255, 0)), port3.X, port3.Y, 20, 20);
      e.Graphics.DrawString (ship1.N.ToString() , aFont, Brushes.Black,ship1.X+18,ship1.Y );
      e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(255, 0, 0)), ship2.X, ship2.Y, 50, 20); //нарисовать элипс с цветом красным  , координатами корабл€1 шириной 50 высотой 20
      e.Graphics.DrawString(ship2.N.ToString(), aFont, Brushes.Black, ship2.X + 18, ship2.Y);

  }

    protected override void OnClosed(EventArgs e)
    {
        ship1.Finish();
    }
}


class Program
    {
        static void Main(string[] args)
            {
                Application.Run(new Window());
            }
    }




//Font aFont = new Font ("Tahoma",12,FontStyle.Regular);
//e.Graphics.DrawString ("“екст1", aFont, Brushes.Black, 10, 30);