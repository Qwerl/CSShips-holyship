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
    //int constsleep = 200;                //скорость потока


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
    private int n, x, y ,sleeptime;
    public int N { get { return n; } }
    public int X { get { return x; } }
    public int Y { get { return y; } }
    public int Sleeptime { get { return sleeptime; } }
    

    public Data(int N, int X, int Y ,int Sleeptime )
    { n = N; x = X; y = Y; sleeptime = Sleeptime; }
}
delegate void DelShip(Data d);//Делегат события


class Port:objectONmap //класс причалов ,отсюда породится 3 порта
{
    Window w;                           //ссылка на окно
    private bool free;                  //индикатор свободности
    private int sleeptime;              //время простаивания в порте
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
            Data d = new Data(N, X, Y ,Sleeptime);
            if (evPort != null)
                evPort(d);
            Thread.Sleep(200);
        }
    }
}

class Ship:objectONmap //класс кораблей
    {
      private bool first;
      private int speed;                //скорость корабля 
      private int xPort, yPort;         //координаты цели(порта)
      private int nPort=1;
      private int thePortSleeptime;
      private int numSledKor;
      //свойства корабля
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
        
        int dx,dy,sleeped;
        
        
        while(life)
        {
            

          //Console.WriteLine("Судно №{0} находиться на координатах Х={1} Y={2} ", N,X,Y);//информация для отладки
            Console.WriteLine("xPort={0}   yPort={1}   thePortSleeptime={2}    # судна{3}    плывём в порт № {4}", xPort, yPort, thePortSleeptime, N, nPort);//информация для отладки
          dx = xPort - x;
          dy = yPort - y;
          if (Math.Abs(xPort - x) < 10 && Math.Abs(yPort - y) < 10)
          {
              if (numSledKor!= 3)
                  numSledKor++;
              else
                  numSledKor=1;
              Console.WriteLine("Судно №{0} прибыло в порт={1}",N,nPort );//информация для отладки
              for (sleeped = 0; sleeped != thePortSleeptime; sleeped += 200)                //механизм остановки в порте
              {
                  if (sleeped + 200 < thePortSleeptime)
                  {
                      //Console.WriteLine("Судну №{0}    осталось простаивать {1}ms", N, thePortSleeptime - sleeped);//информация для отладки
                      Thread.Sleep(200);
                  }
              }

              //first = !first;
              

             

          }
          x += dx / 5;
          y += dy / 5;
          Thread.Sleep(200);
        }
      }
      public void HandlerEv(Data D)
      {
          Data d = (Data)D;
         // if ((d.N==2)||(d.N==3)||(d.N==1))
          if (d.N==numSledKor)
          { this.xPort = d.X; this.yPort = d.Y; this.thePortSleeptime = d.Sleeptime; this.nPort=d.N; }
          //if (nPort==2&&d.N==2)
         //{ this.xPort = d.X; this.yPort = d.Y; this.thePortSleeptime = d.Sleeptime; this.nPort=d.N; }
         // if (nPort==3&&d.N==3)
          //{ this.xPort = d.X; this.yPort = d.Y; this.thePortSleeptime = d.Sleeptime; this.nPort = d.N; }
      }
   }




//окошко
class Window : Form
{
  private int num;//задел на буд
  Port port1, port2, port3;
  Ship ship,ship2;
  Font aFont = new Font("Tahoma", 12, FontStyle.Regular);

  public Window ()
  {

      this.Size = new Size(500, 500);//размер окна
      
      num = 1;//задел на буд

      port1 = new Port(1, 11, 11, 2000, this);   //зелёный порт
      port1.evPort += new DelShip(this.HandlerEv);
      port2 = new Port(2, 200, 350, 2000, this); //синий порт
      port2.evPort += new DelShip(this.HandlerEv);
      port3 = new Port(3, 150, 300, 2000, this);  //желтый порт
      port3.evPort += new DelShip(this.HandlerEv);
      
      ship = new Ship(1, 100, 100, 20);//создать объект корабля с параметрами : первый параметр номер корабля , второй Х ,третий У , четвертый скорость
      port1.evPort += new DelShip(ship.HandlerEv );
      port2.evPort += new DelShip(ship.HandlerEv );
      port3.evPort += new DelShip(ship.HandlerEv);
      
      ship2 = new Ship(20, 1000, 300, 20);//создать объект корабля с параметрами : первый параметр номер корабля , второй Х ,третий У , четвертый скорость
      port1.evPort += new DelShip(ship2.HandlerEv);
      port2.evPort += new DelShip(ship2.HandlerEv);
      port3.evPort += new DelShip(ship2.HandlerEv);
  }

  private void HandlerEv(Data D)
  {
      Invalidate();//перерисовать
  }

  protected override void OnPaint(PaintEventArgs e)
  {
      
      base.OnPaint(e);
      e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(255, 0, 0)), ship.X , ship.Y, 50, 20); //нарисовать элипс с цветом красным  , координатами корабля1 шириной 50 высотой 20
      e.Graphics.DrawString(ship.N.ToString(), aFont, Brushes.Black, ship.X + 18, ship.Y);      //надпись на первый корабль
      e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(0, 255, 0)), port1.X, port1.Y, 20, 20);
      e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(0, 0, 255)), port2.X, port2.Y, 20, 20); 
      e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(255, 255, 0)), port3.X, port3.Y, 20, 20);

      e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(255, 0, 0)), ship2.X, ship2.Y, 50, 20); //нарисовать элипс с цветом красным  , координатами корабля1 шириной 50 высотой 20
      e.Graphics.DrawString(ship2.N.ToString(), aFont, Brushes.Black, ship2.X + 18, ship2.Y);

  }

    protected override void OnClosed(EventArgs e)
    {
        ship.Finish();
        ship2.Finish();
        port1.Finish();
        port2.Finish();
        port3.Finish();
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
//e.Graphics.DrawString ("Текст1", aFont, Brushes.Black, 10, 30);