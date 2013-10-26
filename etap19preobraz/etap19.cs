using System;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using System.Collections;



abstract class objectONmap  //любой объект на карте обладает этими свойствами
{
    private int num;
    protected int x,y;
    protected bool life;
    protected Thread thr;
    public int constsleep = 30;                //скорость потока


    public int N { get { return num; } }
    public int X { get { return x; } }
    public int Y { get { return y; } }

    public objectONmap(int N, int X, int Y)//конструктор
    {
        num = N; x = X; y = Y;
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
    //Window w;                           //ссылка на окно
    private bool free;                  //индикатор свободности
    private int sleeptime;              //время простаивания в порте
    public int Sleeptime
    {
        get { return sleeptime; }
        set { sleeptime = value; }
    }

    public event DelShip evPort;
    public Port(int N, int X, int Y, int Sleeptime)
        :base (N,X,Y)
    {
        
        sleeptime = Sleeptime;
       //thr.Start();
    }

    public override void Move()
    {
        Data d = new Data(N, X, Y, Sleeptime);
        //    if (evPort != null)
          //      evPort(d);
        while (life)
        {
        //    Data d = new Data(N, X, Y ,Sleeptime);
            if (evPort != null)
                evPort(d);
        //    Thread.Sleep(constsleep);
        }
    }
}

class Ship:objectONmap //класс кораблей
{
    private int numNextPort;
    private int speed;                //скорость корабля 
    private int xPort, yPort;         //координаты цели(порта)  
    private int nPort=3;
    private int thePortSleeptime;
    private Color colorTheSHIP;
      
    public Color ShipColor 
    {
        get { return colorTheSHIP;  }
        set { colorTheSHIP = value; }
    }
    //свойства корабля
    public int Speed
    {
        get {return speed;}
        set {speed=value;}
    }

    public Ship(int N, int X, int Y, int Speed, Color ShipColor)//конструктор
        :base (N,X,Y)
    {
        life = true;
        thr = new Thread(new ThreadStart(Move));
        speed = Speed;
        colorTheSHIP = ShipColor;
        thr.Start();
    }

    public override void Move()
    {
        Random randPort;
        randPort = new Random();
        numNextPort = randPort.Next(0, 3);
        //numNextPort=2;

        int dx =0, dy = 0, sleeped;
        int calc = 0;
        while(life)
        {
            //Console.WriteLine("Судно №{0} отправилось из порта-{1}({2} , {3}) в порт-{4})", N, nPort, xPort, xPort, numNextPort);                 //информация для отладки
            //Console.WriteLine("Судно №{0} находиться на координатах Х={1} Y={2} ", N,X,Y);                                                        //информация для отладки
            //Console.WriteLine("{0}   {1}   {2}          {3}", xPort, yPort, thePortSleeptime, N);                                                 //информация для отладки

            if (calc <= 2) //КООООООССССТТЫЫЫЫЫЛЬЬЬ
            {
                dx = xPort - x;
                dy = yPort - y;
                calc++;
            }
            if (Math.Abs(xPort - x) < 50 && Math.Abs(yPort - y) < 50)  //требуется большая погрешность , из-за того  , что много шагов , а координаты целочисленные  ДАЖЕ ПРИ 50 ИНОГДА ПРОМАХИВАЕТСЯ!!
            {                                                          //чем медленнее , тем больше шагод между остравами , там больше вероятность промазать
                calc = 0;
                randPort = new Random();
                numNextPort = randPort.Next(0,3);
                   // Console.WriteLine("Судно №{0} отправилось из порта-{1} в порт-{2}  ", N, nPort, numNextPort);

                    //if (numNextPort!=3)   //1->2->3->1->2->3 и тд
                    //  numNextPort++;
                    //else
                    //  numNextPort=1;

                    //Console.WriteLine("Судно №{0} прибыло в порт {1}",N , nPort );                                                                  //информация для отладки
                for (sleeped = 0; sleeped != thePortSleeptime; sleeped += constsleep)                //механизм остановки в порте
                {
                    if (sleeped + constsleep < thePortSleeptime)
                    {
                        //Console.WriteLine("Судну №{0}    осталось простаивать {1}ms", N, thePortSleeptime - sleeped);                           //информация для отладки
                        Thread.Sleep(constsleep);
                    }
                }
            }
            x += dx * speed / 10000;
            y += dy * speed / 10000;
            Thread.Sleep(constsleep);
        }
    }   

    public void HandlerEv(Data D)
    {
        Data d = (Data)D;
        if (d.N==numNextPort)
        { 
            this.xPort = d.X;
            this.yPort = d.Y;
            this.thePortSleeptime = d.Sleeptime;
            this.nPort=d.N; 
        }
    }
}

class Window : Form //окошко
{
    private int shipCount = 0;              //используется в DEL_ALL и DEL, содержит колличество накликанных кораблей , которые были удалены из списка
    private int numShip;                        //количество кораблей
    //Port port1, port2, port3;               
    ArrayList ships,ports;                        //массив кораблей
    Button butAdd,butDel,butDelAll;         //кнопочки
    ListBox listBox;                        //форма списка
    Random randomSpeed;                     //слчайное число для выдачи кораблю случайной скорости
    Font aFont = new Font("Tahoma", 12, FontStyle.Regular);
  

    public Window ()
    {
        randomSpeed = new Random();
        //свойства окна
        this.Size = new Size(800, 800);//размер окна
        //this.BackColor = Color.Aqua;  //голубой задник , теперь там картинка
        this.BackgroundImage = new Bitmap("C:\\wate2.jpg"); //задник с водой
        

        //this.BackgroundImage = new Bitmap(Ships.Properties.Resources.water);   //ООООЧЕНЬ ЛАГАЕТ
        
        //объекты портов
        ports = new ArrayList();
        for (int i = 0; i < 3; i++)
        {
            if (i == 0)
            {
                int x = 20, y = 20;
                Port port = new Port(i, x, y, 2100);
                port.evPort += new DelShip(this.HandlerEv);
                ports.Add(port);
            }
            if (i == 1)
            {
                int x = 720, y = 20;
                Port port = new Port(i, x, y, 2100);
                port.evPort += new DelShip(this.HandlerEv);
                ports.Add(port);
            }
            if (i == 2)
            {
                int x = 350, y = 650;
                Port port = new Port(i, x, y, 2100);
                port.evPort += new DelShip(this.HandlerEv);
                ports.Add(port);
            }
            //Port port = new Port(i, x , y, 2000);
        }
        /*
        port1 = new Port(1, 20, 20, 2000);
        port1.evPort += new DelShip(this.HandlerEv);

        port2 = new Port(2, 720, 20, 2000);
        port2.evPort += new DelShip(this.HandlerEv);

        port3 = new Port(3, 350, 650, 2000);
        port3.evPort += new DelShip(this.HandlerEv);
        */

        //объекты кораблей
        ships = new ArrayList();
     
        /* теперь это реализовано в ADD
        ship = new Ship(1, 10, 900, 200);//создать объект корабля с параметрами : первый параметр номер корабля , второй Х ,третий У , четвертый скорость
        port1.evPort += new DelShip(ship.HandlerEv );
        port2.evPort += new DelShip(ship.HandlerEv );
        port3.evPort += new DelShip(ship.HandlerEv);

        ship2 = new Ship(2, 300, 900, 500);//создать объект корабля с параметрами : первый параметр номер корабля , второй Х ,третий У , четвертый скорость
        port1.evPort += new DelShip(ship2.HandlerEv);
        port2.evPort += new DelShip(ship2.HandlerEv);
        port3.evPort += new DelShip(ship2.HandlerEv);

        ship3 = new Ship(3, 900, 900, 50);//создать объект корабля с параметрами : первый параметр номер корабля , второй Х ,третий У , четвертый скорость
        port1.evPort += new DelShip(ship3.HandlerEv);
        port2.evPort += new DelShip(ship3.HandlerEv);
        port3.evPort += new DelShip(ship3.HandlerEv);
        */


        //объект кнопки ADD
        butAdd = new Button();

        butAdd.Location = new Point(30, 350);
        butAdd.BackColor = Color.Green;
        butAdd.Size = new Size(45, 20);
        butAdd.Text = "ADD";
        Controls.Add(butAdd);
        butAdd.Click += new EventHandler(Add);

        //объект кнопки DEL
        butDel = new Button();

        butDel.Location = new Point(85, 350);
        butDel.BackColor = Color.Red;
        butDel.Size = new Size(45, 20);
        butDel.Text = "DEL";
        Controls.Add(butDel);
        butDel.Click += new EventHandler(Del);

        //объект кнопки DEL_ALL
        butDelAll = new Button();

        butDelAll.BackColor = Color.DarkRed;
        butDelAll.Location = new Point(30, 375);
        butDelAll.Size = new Size(100, 40);
        butDelAll.Text = "DEL_ALL";
        Controls.Add(butDelAll);
        butDelAll.Click += new EventHandler(DelAll);

        //объект интерфейсного элемента списка(прямоугольник с цифрами-номерами кораблей)
        listBox = new ListBox();
        listBox.Location = new Point(30, 420);
        listBox.Size = new Size(100, 340);
        Controls.Add(listBox);
        listBox.MouseClick += new MouseEventHandler(ListBox);
    }

    private void HandlerEv(Data D)
    {
        Invalidate();//перерисовать
    }

    private void ListBox(object obj, EventArgs args)
    {
        //Console.WriteLine("Я тут");
        int numSel = (int)listBox.SelectedItem;
        for (int i = 0; i < ships.Count; i++)
        {
            Ship ship = (Ship)ships[i];
            if (ship.N == numSel)
            {
                Console.WriteLine("скорость корабля № {0} = {1}",ship.N , ship.Speed);
                ship.ShipColor = Color.DarkCyan;
            }
        }
    }

    private void Add(object obj, EventArgs args)
    {
        numShip++;
        int randomSpeedColor = randomSpeed.Next(100, 500);
        Ship ship = new Ship(numShip, 500, 500, randomSpeedColor, Color.FromArgb(randomSpeedColor / 2, 0, 0));
        for (int i = 0; i < ports.Count; i++)
        {
            Port port = (Port)ports[i];
            port.evPort += new DelShip(ship.HandlerEv);
        }
        /*
        port1.evPort += new DelShip(ship.HandlerEv);
        port2.evPort += new DelShip(ship.HandlerEv);
        port3.evPort += new DelShip(ship.HandlerEv);
        */
        ships.Add(ship);
        listBox.Items.Add(ship.N);
    }

    private void Del(object obj, EventArgs args)
    {
        if (listBox.Items.Count == 0)
            MessageBox.Show("Сначала создайте хотябы один корабль", "Ошибка");
        else
        {
            if (listBox.SelectedIndex == -1)
                MessageBox.Show("Выберите номер удаляемого корабля из списка", "Ошибка");
            else
            {
                int numSel = (int)listBox.SelectedItem;
                for (int i = 0; i < ships.Count; i++)
                {
                    Ship ship = (Ship)ships[i];
                    if (ship.N == numSel)
                    {
                        shipCount++;
                        ships.Remove(ship);
                        listBox.Items.Remove(numSel);
                        ship.Finish();
                    }
                }
            }
        }
    }

    private void DelAll(object obj, EventArgs args)
    {
        if (ships.Count == 0)
            MessageBox.Show("не создано ни одного корабля", "Ошибка");
        else
        {
            shipCount += ships.Count;
            Console.WriteLine("shipCount = {0}", shipCount);
                                                                                                    /*for (int i = 0, j = 1; j <= shipCount; j++)  // i для работы с массивом кораблей  j для рабыты со списком
                                                                                                      {
                                                                                                          Ship ship = (Ship)ships[i];
                                                                                                          ships.Remove(ship);
                                                                                                          ship.Finish();
                                                                                                          listBox.Items.Remove(j);
                                                                                                          Console.WriteLine("3for = {0}", j);
                                                                                                      }
                                                                                                   */
            for (int i = 0; i <= shipCount; i++) //убийца списка
            {
                listBox.Items.Remove(i);
                                                                                                    //i--;
                                                                                                    //if (i<ships.Count)
                                                                                                    //{
                                                                                                    //Ship ship = (Ship)ships[i];
                                                                                                    //ships.Remove(ship);
                                                                                                    //ship.Finish();
            }
                                                                                                    //    Console.WriteLine("1for = {0}", i);



            for (int i = 0; i < ships.Count; i++) //убийца кораблей    принцип работы:цикл крутиться , пока ships.Count не станет нулём
            {

                if (i < ships.Count)
                {
                    Ship ship = (Ship)ships[i];
                    ships.Remove(ship);
                    ship.Finish();
                    //Console.WriteLine("2for = {0}", i);
                    i--;
                }
                else
                {
                                                                                                    //Console.WriteLine("break");
                    break;//не помню , может ли сюда вообще попасть в ходе выполнения , поэтому пока оставляю
                }
                //Console.WriteLine("3for = {0}", i);
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;

        for (int i = 0; i < ports.Count; i++)
        {
            Port port = (Port)ports[i];
            if (port.N == 0)
            {
                g.DrawImage(Image.FromFile("C:\\PORTprozrachn_TEN2.jpg"), new Point(port.X - 30, port.Y));
                //Console.WriteLine("narisoval 1");
            }
            if (port.N == 1)
            {
                g.DrawImage(Image.FromFile("C:\\PORTprozrachn_TEN2.jpg"), new Point(port.X - 50, port.Y));
                //Console.WriteLine("narisoval 2"); 
            }
            if (port.N == 2)
            {
                g.DrawImage(Image.FromFile("C:\\PORTprozrachn_TEN2.jpg"), new Point(port.X, port.Y - 50));
                //Console.WriteLine("narisoval 3");
            }
        }
        /* 
        g.DrawImage(Ships.Properties.Resources.PORTprozrachn_TEN_, new Point(port1.X - 30, port.Y));
        g.DrawImage(Ships.Properties.Resources.PORTprozrachn_TEN_, new Point(port2.X - 50, port2.Y));
        g.DrawImage(Ships.Properties.Resources.PORTprozrachn_TEN_, new Point(port3.X, port3.Y - 50));
        */

        /* теперь всё сделано через картинки или через кнопку , этот код на всякий случай оставил
        //e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(0, 255, 0)), port1.X, port1.Y, 50, 50);      //Порт1
        //e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(0, 0, 255)), port2.X, port2.Y, 50, 50);      //Порт2
        //e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(255, 255, 0)), port3.X, port3.Y, 50, 50);    //Порт3
          
        e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(255, 0, 0)), ship.X, ship.Y, 50, 20); //нарисовать элипс с цветом красным  , координатами корабля1 шириной 50 высотой 20
        e.Graphics.DrawString(ship.N.ToString(), aFont, Brushes.Black, ship.X + 18, ship.Y);      //надпись на первый корабль

        e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(255, 0, 0)), ship2.X, ship2.Y, 50, 20); //нарисовать элипс с цветом красным  , координатами корабля1 шириной 50 высотой 20
        e.Graphics.DrawString(ship2.N.ToString(), aFont, Brushes.Black, ship2.X + 18, ship2.Y);

        e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(255, 0, 0)), ship3.X, ship3.Y, 50, 20); //нарисовать элипс с цветом красным  , координатами корабля1 шириной 50 высотой 20
        e.Graphics.DrawString(ship3.N.ToString(), aFont, Brushes.Black, ship3.X + 18, ship3.Y);
        */

        for (int i = 0; i < ships.Count; i++)
        {
            Ship ship = (Ship)ships[i];
            e.Graphics.FillEllipse(new SolidBrush(ship.ShipColor), ship.X, ship.Y, 50, 20);           //нарисовать элипс с:  1)цветом красным (яркость зависит от скорости) 2.3)координатами корабля 4)шириной 50 5)высотой 20
            e.Graphics.DrawString(ship.N.ToString(), aFont, Brushes.Black, ship.X + 18, ship.Y);      //надпись на первый корабль
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        for (int i = 0; i < ships.Count; i++)
        {
            Ship ship = (Ship)ships[i];
            ship.Finish();
        }
        for (int i=0 ; i< ports.Count;i++)
        {
            Port port = (Port)ports[i];
            port.Finish();
        }
        //port3.Finish();
    }
}

class Program
    {
        static void Main(string[] args)
            {
                Window w1 = new Window();
                //Window w2 = new Window();  //потом станет вторым окошком
                Application.Run(w1);
            }
    }