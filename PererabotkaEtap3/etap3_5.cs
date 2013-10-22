using System;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;



abstract class objectONmap  //любой объект на карте обладает этими свойствами
{
    private int num;
    protected int x,y;
    protected bool life;
    protected Thread thr;

    public int N { get { return num; } }
    public int X { get { return x; } }
    public int Y { get { return y; } }

    public objectONmap(int N, int X, int Y)//конструктор
    {
        num = N; x = X; y = Y;
        life = true;
        thr = new Thread(new ThreadStart(Move));
    }

    abstract public void Move();

    public void Finish() { life = false; }
}
class Data : EventArgs //класс данных событий кораблей
{
    private int n, x, y;
    public int N { get { return n; } }
    public int X { get { return x; } }
    public int Y { get { return y; } }

    public Data(int N, int X, int Y)
    { n = N; x = X; y = Y; }
}
delegate void DelShip(Data d);//ƒелегат событи€


class Port:objectONmap //класс причалов ,отсюда породитс€ 3 порта
{
    Window w;                           //ссылка на окно
    private bool free;                  //индикатор свободности
    private int sleeptime;              //врем€ простаивани€ в порте
    public int Sleeptime
    {
        get { return sleeptime; }
        set { sleeptime = value; }
    }

    public event DelShip evPort;
    public Port(int N, int X, int Y, int Sleeptime, Window W)
        :base (N,X,Y)
    {
        w = W;
        sleeptime = Sleeptime;
        thr.Start();
    }

    public override void Move()
    {
        while (life)
        {
            Data d = new Data(N, X, Y);
            if (evPort != null)
                evPort(d);
            Thread.Sleep(200);
        }
    }
    // public void HandlerEv(Data D)
    // {
    //    Data d = (Data)D;
    //    this.xPort = d.X; this.yPort = d.Y;
    //}
}

class Ship:objectONmap //класс кораблей
    {
      private bool first;
      private int speed;                //скорость корабл€ 
      int constsleep=50;                //скорость потока
      private int xPort, yPort;         //координаты цели(порта)  
      
      //свойства корабл€
      public int Speed
      {
         get {return speed;}
         set {speed=value;}
      }

      public Ship(int N, int X, int Y, int Speed)//конструктор
          :base (N,X,Y)
      {
        speed = Speed;
        thr.Start();
      }

      public override void Move()
      { 
        int dx,dy;
        
        while(life)
        { 
          System.Console.WriteLine("{0}   {1}",xPort,yPort);
          dx = xPort - x;
          dy = yPort - y;
          if (Math.Abs(xPort - x) < 10 && Math.Abs(yPort - y) < 10)
              first = !first;
          x += dx / 5;
          y += dy / 5;
          Thread.Sleep(200);


         //x += dx;
         //if ((x < 0 || x > w.ClientSize.Width) || (y < 0 || y > w.ClientSize.Height)) //отталкивание от стенок
         //{ dx = -dx; dy = -dy; }
         //Data d = new Data(N, X, Y);
         //  if (evShip != null)
         //       evShip(d); 
        }
      }
      public void HandlerEv(Data D)
      {
          Data d = (Data)D;
          if (first&&d.N==1||!first&&d.N==2)
          {this.xPort=d.X; this.yPort=d.Y;}
      }
   }




//окошко
class Window : Form
{
  private int num;
  Port port1, port2;// port3;
  Ship ship;//ship2;
  Font aFont = new Font("Tahoma", 12, FontStyle.Regular);

  public Window ()
  {
      num = 1; 
      port1 = new Port(1, 20, 20, 200, this);
      port1.evPort += new DelShip(this.HandlerEv);
      port2 = new Port(2, 230, 230, 200, this);
      port2.evPort += new DelShip(this.HandlerEv);
      //port3 = new Port(1, 50, 300,200,this);
      //port3.evShip += new DelShip(this.HandlerShip);
      
      ship = new Ship(1, 100, 100, 20);//создать объект корабл€ с параметрами : первый параметр номер корабл€ , второй ’ ,третий ” , четвертый скорость
      port1.evPort += new DelShip(ship.HandlerEv );
      port2.evPort += new DelShip(ship.HandlerEv );
      //ship2 = new Ship(2, 300, 300, 20, this);//создать объект корабл€ с параметрами : первый параметр номер корабл€ , второй ’ ,третий ” , четвертый скорость
      //ship2.evShip += new DelShip(this.HandlerShip);
  }

  private void HandlerEv(Data D)
  {
      Invalidate();//перерисовать
  }

  protected override void OnPaint(PaintEventArgs e)
  {
      
      base.OnPaint(e);
      e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(255, 0, 0)), ship.X , ship.Y, 50, 20); //нарисовать элипс с цветом красным  , координатами корабл€1 шириной 50 высотой 20
      e.Graphics.DrawString(ship.N.ToString(), aFont, Brushes.Black, ship.X + 18, ship.Y);      //надпись на первый корабль
      e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(0, 255, 0)), port1.X, port1.Y, 20, 20);
      e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(0, 0, 250)), port2.X, port2.Y, 20, 20); 
     // e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(255, 255, 0)), port3.X, port3.Y, 20, 20);

      //e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(255, 0, 0)), ship2.X, ship2.Y, 50, 20); //нарисовать элипс с цветом красным  , координатами корабл€1 шириной 50 высотой 20
      //e.Graphics.DrawString(ship2.N.ToString(), aFont, Brushes.Black, ship2.X + 18, ship2.Y);

  }

    protected override void OnClosed(EventArgs e)
    {
        ship.Finish();
        port1.Finish();
        port2.Finish();
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