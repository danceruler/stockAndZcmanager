using Caliburn.Micro;
using Panuon.UI;
using Stock;
using StockView.Help;
using StockView.Model;
using StockView.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StockView
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Canvas canvas;
        private Canvas canvas2;
        private PUBubble Info_TextBox;
        private PUBubble DifDeaInfo_TextBox;


        private bool IsDes_Info_TextBox = true;
        private bool IsDes_DifDeaInfo_TextBox = true;

        //cord
        int cord_height = 200;
        int cord_width = 800;
        //点半径
        private int r = 4;
        //坐标轴起点坐标
        int left_start_x = 50;
        int left_start_y = 250;
        //横轴坐标缩放比例
        double cord_left = 1;
        //纵轴坐标最小和最大代表的数值
        decimal min_y = 5;
        decimal max_y = 10;
        //纵坐标代表数值x与实际数值y的k和b
        decimal k;
        decimal b;

        decimal k2;
        decimal b2;
        //现在的点数量
        int pointsum;
        //筛选范围
        DateTime startDate;
        DateTime endDate;
        //是否正在更新table
        bool isUodateTable = false;
        //表中是否有数据
        bool isdataInTable = false;
        //表2显示macd或成交量
        int MACDorVolume = 1;



        //各新闻模块
        string[] newstypes = new string[5] { "融资公告", "风险提示", "信息变更", "资产重组", "持股变动" };
        //融资公告
        ScrollViewer finacescrollViewer;
        StackPanel finace = new StackPanel();
        //风险提示
        ScrollViewer dangerscrollViewer;
        StackPanel danger = new StackPanel();
        //信息变更
        ScrollViewer infochangescrollViewer;
        StackPanel infochange = new StackPanel();
        //资产重组
        ScrollViewer recomboscrollViewer;
        StackPanel recombo = new StackPanel();
        //持股变动
        ScrollViewer havestockchangescrollViewer;
        StackPanel havestockchange = new StackPanel();

        //推荐股票
        ScrollViewer HotStockscrollViewer;
        StackPanel HotStock = new StackPanel();

        public List<List_MACDModel> listModels = new List<List_MACDModel>();

        private StockSupportSystemEntities db = new StockSupportSystemEntities();

        public MainWindow(）
        {
            InitializeComponent();
            CreatCanvas();
            InitEvents();
            Thread thread = new Thread(() =>
            {
                //StockNewsHelp.initStockNews();
                InitNews();
            });
            thread.IsBackground = true;
            thread.Start();
        }
        public void InitEvents()
        {
            MainViewModel mainViewModel = new MainViewModel(this);
            this.DataContext = mainViewModel;

        }
        public void CreatCanvas()
        {
            canvas = new Canvas();
            //设置canvas位置
            MainGrid.Children.Add(canvas);
            Grid.SetRow(canvas, 1);
            canvas.Width = this.Width - 600;
            canvas.Height = 320;
            canvas.Margin = new Thickness(15, 20, 0, 0);
            canvas.HorizontalAlignment = HorizontalAlignment.Left;
            canvas.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF000000"));


            canvas2 = new Canvas();
            MainGrid.Children.Add(canvas2);
            Grid.SetRow(canvas2, 3);
            canvas2.Width = this.Width - 600;
            canvas2.Height = 300;
            canvas2.Margin = new Thickness(15, 10, 0, 0);
            canvas2.HorizontalAlignment = HorizontalAlignment.Left;
            canvas2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF000000"));

            DrawCord();
        }

        //导入新闻数据和热门股票数据
        public void CreateNewsBoxs()
        {
            for (int i = 0; i < newstypes.Length; i++)
            {
                string newstype = newstypes[i];
                List<StockNews> stockNews = db.StockNews.Where(t => t.type == newstype).ToList();
                if (newstype == "持股变动")
                {
                    stockNews = db.StockNews.Where(t => t.type == newstype).OrderByDescending(t => t.sort).ToList();
                }else if(newstype == "风险提示")
                {
                    stockNews = new List<StockNews>();
                    stockNews.AddRange(db.StockNews.Where(t => t.type == newstype && t.sort > 100).OrderByDescending(t => t.Date).ToList());
                    stockNews.AddRange(db.StockNews.Where(t => t.type == newstype && t.sort <= 100).OrderByDescending(t => t.Date).ToList());
                }else if (newstype == "资产重组")
                {
                    stockNews = new List<StockNews>();
                    stockNews.AddRange(db.StockNews.Where(t => t.type == newstype && t.sort > 2).OrderByDescending(t => t.Date).ToList());
                    stockNews.AddRange(db.StockNews.Where(t => t.type == newstype && t.sort <= 2).OrderByDescending(t => t.Date).ToList());
                }
                switch (i)
                {
                    case 0:
                        finacescrollViewer = new ScrollViewer();
                        finacescrollViewer.Width = 400;
                        finacescrollViewer.Height = 730;
                        finacescrollViewer.Content = finace;
                        var cards = GetNewsCard(stockNews);
                        foreach (StackPanel pc in cards)
                        {
                            finace.Children.Add(pc);
                        }
                        break;
                    case 1:
                        dangerscrollViewer = new ScrollViewer();
                        dangerscrollViewer.Width = 400;
                        dangerscrollViewer.Height = 730;
                        dangerscrollViewer.Content = danger;
                        var dangercards = GetNewsCard(stockNews);
                        foreach (StackPanel pc in dangercards)
                        {
                            danger.Children.Add(pc);
                        }
                        break;
                    case 2:
                        infochangescrollViewer = new ScrollViewer();
                        infochangescrollViewer.Width = 400;
                        infochangescrollViewer.Height = 730;
                        infochangescrollViewer.Content = infochange;
                        var infochangecards = GetNewsCard(stockNews);
                        foreach (StackPanel pc in infochangecards)
                        {
                            infochange.Children.Add(pc);
                        }
                        break;
                    case 3:
                        recomboscrollViewer = new ScrollViewer();
                        recomboscrollViewer.Width = 400;
                        recomboscrollViewer.Height = 730;
                        recomboscrollViewer.Content = recombo;
                        var recombocards = GetNewsCard(stockNews);
                        foreach (StackPanel pc in recombocards)
                        {
                            recombo.Children.Add(pc);
                        }
                        break;
                    case 4:
                        havestockchangescrollViewer = new ScrollViewer();
                        havestockchangescrollViewer.Width = 400;
                        havestockchangescrollViewer.Height = 730;
                        havestockchangescrollViewer.Content = havestockchange;
                        var havestockchangecards = GetNewsCard(stockNews);
                        foreach (StackPanel pc in havestockchangecards)
                        {
                            havestockchange.Children.Add(pc);
                        }
                        break;
                    default:
                        break;
                }
            }
            string sql = "with T as(select id, date, name, InOutPoint, ROW_NUMBER() over(partition by id order by date desc) as rowNum from[StockSupportSystem].[dbo].[StockInfo] where InOutPoint = 1 or InOutPoint = 2) select* from T where rowNum = 1 and InOutPoint = 1";

            var hotstocks = db.Database.SqlQuery<HotStock>(sql).ToList();
            HotStockscrollViewer = new ScrollViewer();
            HotStockscrollViewer.Width = 400;
            HotStockscrollViewer.Height = 730;
            HotStockscrollViewer.Content = HotStock;
            var hotstockcards = getHotCard(hotstocks);
            foreach (StackPanel pc in hotstockcards)
            {
                HotStock.Children.Add(pc);
            }

        }
        public void InitNews()
        {
            System.Action test = () =>
            {
                this.loading.Height = 0;
                CreateNewsBoxs();
                BindableCollection<PUTabItemModel> TabItems = new BindableCollection<PUTabItemModel>()
            {
                new PUTabItemModel() {  Header = "股票推荐",Value =0, Content = HotStockscrollViewer,  },
                new PUTabItemModel() {  Header = "持股好空性",Value =3, Content = havestockchangescrollViewer,  },
                new PUTabItemModel() {  Header = "风险好空性",Value =2, Content = dangerscrollViewer,  },
                new PUTabItemModel() {  Header = "资产类",Value =2, Content = recomboscrollViewer,  },
                new PUTabItemModel() {  Header = "信息变更",Value =3, Content = infochangescrollViewer,  },
                new PUTabItemModel() {  Header = "融资公告",Value =1, Content = finacescrollViewer,  },
                


            };
                this.news_Tab.BindingItems = TabItems;
            };
            this.Dispatcher.BeginInvoke(test);
            Thread.Sleep(50);
        }
        #region paint

        public List<StackPanel> GetNewsCard(List<StockNews> stockNews)
        {
            List<StackPanel> result = new List<StackPanel>();
            int cnd = 1;
            foreach (StockNews news in stockNews)
            {
                StackPanel stackPanel = new StackPanel();
                stackPanel.Height = 80;
                stackPanel.Width = 400;
                PUTextBox newsCard = new PUTextBox();
                newsCard.Height = 50;
                newsCard.Width = 400;
                newsCard.BorderCornerRadius = new CornerRadius(5, 5, 0, 0);
                newsCard.VerticalAlignment = VerticalAlignment.Center;
                newsCard.IsEnabled = false;
                newsCard.Text = "股票代码：" + news.code + "   股票名称：" + news.stockname + "   日期：" + news.Date.Value.ToString("yyyy年MM月dd日");
                newsCard.Foreground = Brushes.Black;
                if (news.sort > 100)
                {
                    if (news.title.Contains("增持"))
                    {
                        newsCard.Background = Brushes.Pink;
                        if (news.sort > 10000000)
                        {
                            stackPanel.Height += 25;
                            Label label = new Label();
                            label.FontWeight = FontWeights.Bold;
                            label.FontStyle = FontStyles.Italic;
                            label.Width = 100;
                            label.Content = "重大利好";
                            label.HorizontalAlignment = HorizontalAlignment.Left;
                            label.Background = Brushes.Pink;
                            label.Foreground = Brushes.Black;
                            stackPanel.Children.Add(label);
                        }
                        else if (500000<news.sort&&news.sort<10000000)
                        {
                            stackPanel.Height += 25;
                            Label label = new Label();
                            label.FontWeight = FontWeights.Bold;
                            label.FontStyle = FontStyles.Italic;
                            label.Width = 100;
                            label.Content = "一般利好";
                            label.HorizontalAlignment = HorizontalAlignment.Left;
                            label.Background = Brushes.Pink;
                            label.Foreground = Brushes.Black;
                            stackPanel.Children.Add(label);
                        }
                    }
                    else if (news.title.Contains("减持"))
                    {
                        newsCard.Background = Brushes.LightGreen;
                        if (news.sort > 10000000)
                        {
                            stackPanel.Height += 25;
                            Label label = new Label();
                            label.FontWeight = FontWeights.Bold;
                            label.FontStyle = FontStyles.Italic;
                            label.Width = 100;
                            label.Content = "重大利空";
                            label.HorizontalAlignment = HorizontalAlignment.Left;
                            label.Background = Brushes.LightGreen;
                            label.Foreground = Brushes.Black;
                            stackPanel.Children.Add(label);
                        }
                        else if (500000 < news.sort && news.sort < 10000000)
                        {
                            stackPanel.Height += 25;
                            Label label = new Label();
                            label.FontWeight = FontWeights.Bold;
                            label.FontStyle = FontStyles.Italic;
                            label.Width = 100;
                            label.Content = "一般利空";
                            label.HorizontalAlignment = HorizontalAlignment.Left;
                            label.Background = Brushes.LightGreen;
                            label.Foreground = Brushes.Black;
                            stackPanel.Children.Add(label);
                        }
                    }
                }
                if(news.type == "风险提示")
                {
                    if (news.sort == 101)
                    {
                        newsCard.Background = Brushes.Pink;
                        if (cnd <= 5)
                        {
                            stackPanel.Height += 25;
                            Label label = new Label();
                            label.FontWeight = FontWeights.Bold;
                            label.FontStyle = FontStyles.Italic;
                            label.Width = 100;
                            label.Content = "重大利好";
                            label.HorizontalAlignment = HorizontalAlignment.Left;
                            label.Background = Brushes.Pink;
                            label.Foreground = Brushes.Black;
                            stackPanel.Children.Add(label);
                        } else if (5 < cnd && cnd <= 10)
                        {
                            stackPanel.Height += 25;
                            Label label = new Label();
                            label.FontWeight = FontWeights.Bold;
                            label.FontStyle = FontStyles.Italic;
                            label.Width = 100;
                            label.Content = "一般利好";
                            label.HorizontalAlignment = HorizontalAlignment.Left;
                            label.Background = Brushes.Pink;
                            label.Foreground = Brushes.Black;
                            stackPanel.Children.Add(label);
                        }
                    }else if(news.sort == 102)
                    {
                        newsCard.Background = Brushes.LightGreen;
                        if (cnd <= 5)
                        {
                            stackPanel.Height += 25;
                            Label label = new Label();
                            label.FontWeight = FontWeights.Bold;
                            label.FontStyle = FontStyles.Italic;
                            label.Width = 100;
                            label.Content = "重大利空";
                            label.HorizontalAlignment = HorizontalAlignment.Left;
                            label.Background = Brushes.LightGreen;
                            label.Foreground = Brushes.Black;
                            stackPanel.Children.Add(label);
                        }
                        else if (5 < cnd && cnd <= 10)
                        {
                            stackPanel.Height += 25;
                            Label label = new Label();
                            label.FontWeight = FontWeights.Bold;
                            label.FontStyle = FontStyles.Italic;
                            label.Width = 100;
                            label.Content = "一般利好";
                            label.HorizontalAlignment = HorizontalAlignment.Left;
                            label.Background = Brushes.LightGreen;
                            label.Foreground = Brushes.Black;
                            stackPanel.Children.Add(label);
                        }
                    }
                }
          
                if(news.type == "资产重组")
                {
                    if (news.title.Contains("转让") || news.title.Contains("出售"))
                    {
                        newsCard.Background = Brushes.Pink;
                        if (cnd <= 5)
                        {
                            stackPanel.Height += 25;
                            Label label = new Label();
                            label.FontWeight = FontWeights.Bold;
                            label.FontStyle = FontStyles.Italic;
                            label.Width = 100;
                            label.Content = "重大利好";
                            label.HorizontalAlignment = HorizontalAlignment.Left;
                            label.Background = Brushes.Pink;
                            label.Foreground = Brushes.Black;
                            stackPanel.Children.Add(label);
                        }
                        else if (5 < cnd && cnd <= 10)
                        {
                            stackPanel.Height += 25;
                            Label label = new Label();
                            label.FontWeight = FontWeights.Bold;
                            label.FontStyle = FontStyles.Italic;
                            label.Width = 100;
                            label.Content = "一般利好";
                            label.HorizontalAlignment = HorizontalAlignment.Left;
                            label.Background = Brushes.Pink;
                            label.Foreground = Brushes.Black;
                            stackPanel.Children.Add(label);
                        }
                    }
                    else if (news.title.Contains("回购") || news.title.Contains("收购") || news.title.Contains("购买"))
                    {
                        newsCard.Background = Brushes.LightGreen;
                        if (cnd <= 5)
                        {
                            stackPanel.Height += 25;
                            Label label = new Label();
                            label.FontWeight = FontWeights.Bold;
                            label.FontStyle = FontStyles.Italic;
                            label.Width = 100;
                            label.Content = "重大利空";
                            label.HorizontalAlignment = HorizontalAlignment.Left;
                            label.Background = Brushes.LightGreen;
                            label.Foreground = Brushes.Black;
                            stackPanel.Children.Add(label);
                        }
                        else if (5 < cnd && cnd <= 10)
                        {
                            stackPanel.Height += 25;
                            Label label = new Label();
                            label.FontWeight = FontWeights.Bold;
                            label.FontStyle = FontStyles.Italic;
                            label.Width = 100;
                            label.Content = "一般利空";
                            label.HorizontalAlignment = HorizontalAlignment.Left;
                            label.Background = Brushes.LightGreen;
                            label.Foreground = Brushes.Black;
                            stackPanel.Children.Add(label);
                        }
                    }
                }
                Label titleText = new Label();
                titleText.Width = 400;
                titleText.Height = 30;
                titleText.HorizontalAlignment = HorizontalAlignment.Center;
                Run run = new Run(news.title);
                Hyperlink hyperlink = new Hyperlink(run);
                hyperlink.NavigateUri = new Uri(news.url);
                hyperlink.Click += Hyperlink_Click;
                hyperlink.DataContext = news;
                titleText.Content = hyperlink;

                stackPanel.Children.Add(newsCard);
                stackPanel.Children.Add(titleText);
                result.Add(stackPanel);
                cnd++;
            }
            return result;
        }
        public List<StackPanel> getHotCard(List<HotStock> hotStocks)
        {
            List<StackPanel> result = new List<StackPanel>();
            foreach (HotStock news in hotStocks)
            {
                if (news.date.AddMonths(4) < DateTime.Now) continue;
                StackPanel stackPanel = new StackPanel();
                stackPanel.Height = 80;
                stackPanel.Width = 400;
                stackPanel.Orientation = Orientation.Horizontal;
                PUCard pUCard = new PUCard();
                pUCard.Height = 100;
                pUCard.Width = 320;
                pUCard.Content = "股票代码：" + news.id + " 股票名称：" + news.name + " \n日期：" + news.date.ToString("yyyy年MM月dd日");
                pUCard.Background = Brushes.Wheat;
                pUCard.Foreground = Brushes.Black;
                pUCard.VerticalAlignment = VerticalAlignment.Center;
                pUCard.HorizontalAlignment = HorizontalAlignment.Center;

                PUButton pUButton = new PUButton();
                pUButton.Height = 100;
                pUButton.Width = 80;
                pUButton.BorderCornerRadius = new CornerRadius(0, 5, 5, 0);
                pUButton.Background = Brushes.LightGray;
                pUButton.DataContext = news;
                pUButton.Click += TransToDate;
                pUButton.Content = "跳转";

                stackPanel.Children.Add(pUCard);
                stackPanel.Children.Add(pUButton);
                result.Add(stackPanel);
            }
            return result;
        }

        public void AddPoint(int start_x, int start_y, double x, decimal y, double r, Canvas canvas, MyPoint myPoint, string color)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Height = 2 * r;
            ellipse.Width = 2 * r;
            ellipse.DataContext = myPoint;
            ellipse.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            ellipse.MouseDown += ShowPointInfo;
            ellipse.MouseUp += HidePointInfo;
            canvas.Children.Add(ellipse);
            Canvas.SetLeft(ellipse, start_x + (cord_left * x - r));
            Canvas.SetTop(ellipse, (double)k * (double)y + (double)b + 20);
        }
        public void InfoRec(MyPoint myPoint, double x, double r, double a)
        {
            Rectangle rectangle = new Rectangle();
            rectangle.Height = Math.Abs((double)k * (double)(myPoint.closeP - myPoint.openP));
            rectangle.Width = cord_width / (a * 1.1);
            if (myPoint.openP >= myPoint.closeP)
            {
                rectangle.Fill = Brushes.Green;
                rectangle.SetValue(Canvas.TopProperty, (double)k * (double)myPoint.openP + (double)b + 40);
            }
            else
            {
                rectangle.Fill = Brushes.Red;
                rectangle.SetValue(Canvas.TopProperty, (double)k * (double)myPoint.closeP + (double)b + 40);
            }
            if (myPoint.InOutP == 1)
            {
                rectangle.Fill = Brushes.White;
            }
            if (myPoint.InOutP == 2)
            {
                rectangle.Fill = Brushes.Yellow;
            }
            rectangle.SetValue(Canvas.LeftProperty, left_start_x + (cord_left * x - r));

            rectangle.MouseMove += ShowPointInfo;
            rectangle.MouseLeave += HidePointInfo;
            rectangle.DataContext = myPoint;
            canvas.Children.Add(rectangle);
        }
        public void AddLine(int start_x, int start_y, double x1, decimal y1, double x2, decimal y2, double r, SolidColorBrush brushes, double right_pianyi)
        {
            Line line = new Line();
            line.Stroke = brushes;
            line.StrokeThickness = 1;
            line.X1 = start_x + x1 + right_pianyi;
            line.Y1 = (double)k * (double)y1 + (double)b + 40;
            line.X2 = start_x + x2 + right_pianyi;
            line.Y2 = (double)k * (double)y2 + (double)b + 40;
            canvas.Children.Add(line);
        }
        public void AddMACDLine(MyPoint myPoint, int start_x, int start_y, double x1, decimal y1, double x2, decimal y2, double r, SolidColorBrush brushes)
        {
            Line line = new Line();
            line.Stroke = brushes;
            line.StrokeThickness = 2;
            line.X1 = start_x + x1;
            line.Y1 = (double)k2 * (double)y1 + (double)b2 + 40;
            line.X2 = start_x + x2;
            line.Y2 = (double)k2 * (double)y2 + (double)b2 + 40;
            line.DataContext = myPoint;
            line.MouseMove += ShowDifDea;
            line.MouseLeave += HideDifDea;
            canvas2.Children.Add(line);
        }
        public void AddVolumeRec(MyPoint myPoint, int start_x, int start_y, double x1, decimal y1, double r, SolidColorBrush brushes)
        {
            Rectangle rectangle = new Rectangle();
            rectangle.Height = 250 - ((double)k2 * (double)y1 + (double)b2 + 40);
            rectangle.Width = cord_width / (pointsum * 1.1);
            if (myPoint.closeP > myPoint.openP)
            {
                rectangle.Fill = Brushes.Red;
            }
            else
            {
                rectangle.Fill = Brushes.Green;
            }

            rectangle.SetValue(Canvas.TopProperty, ((double)k2 * (double)y1 + (double)b2 + 40));
            rectangle.SetValue(Canvas.LeftProperty, left_start_x + (cord_left * x1 - r));
            rectangle.DataContext = myPoint;
            rectangle.MouseMove += ShowVolume;
            rectangle.MouseLeave += HideVolume;
            canvas2.Children.Add(rectangle);
        }

        public void DrawCord()
        {
            Line Yline = new Line();
            Yline.Stroke = Brushes.White;
            Yline.StrokeThickness = 1.5;
            Yline.X1 = 40;
            Yline.Y1 = 270;
            Yline.X2 = 40;
            Yline.Y2 = 20;

            Line Xline = new Line();
            Xline.Stroke = Brushes.White;
            Xline.StrokeThickness = 1.5;
            Xline.X1 = 40;
            Xline.Y1 = 270;
            Xline.X2 = 880;
            Xline.Y2 = 270;
            canvas.Children.Add(Yline);
            canvas.Children.Add(Xline);

            Line Yline2 = new Line();
            Yline2.Stroke = Brushes.White;
            Yline2.StrokeThickness = 1.5;
            Yline2.X1 = 40;
            Yline2.Y1 = 270;
            Yline2.X2 = 40;
            Yline2.Y2 = 20;

            Line Xline2 = new Line();
            Xline2.Stroke = Brushes.White;
            Xline2.StrokeThickness = 1.5;
            Xline2.X1 = 40;
            Xline2.Y1 = 270;
            Xline2.X2 = 880;
            Xline2.Y2 = 270;
            canvas2.Children.Add(Yline2);
            canvas2.Children.Add(Xline2);
        }
        public void DrawCord(List<MyPoint> points)
        {
            Line Yline = new Line();
            Yline.Stroke = Brushes.White;
            Yline.StrokeThickness = 1.5;
            Yline.X1 = 40;
            Yline.Y1 = 270;
            Yline.X2 = 40;
            Yline.Y2 = 20;

            Line Xline = new Line();
            Xline.Stroke = Brushes.White;
            Xline.StrokeThickness = 1.5;
            Xline.X1 = 40;
            Xline.Y1 = 270;
            Xline.X2 = 880;
            Xline.Y2 = 270;
            canvas.Children.Add(Yline);
            canvas.Children.Add(Xline);

            string month = "";
            int getlength = 7;
            if (startDate.AddMonths(5) < endDate)
            {
                getlength = 4;
            }
            for (int i = 0; i < points.Count(); i++)
            {
                if (points[i].date.Substring(0, getlength) != month)
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = points[i].date.Substring(0, getlength);
                    textBlock.Foreground = Brushes.Wheat;
                    Canvas.SetLeft(textBlock, left_start_x + (cord_left * (double)(i + 1) / (double)(points.Count() + 1) * cord_width - r));
                    Canvas.SetTop(textBlock, 290);
                    canvas.Children.Add(textBlock);
                    month = points[i].date.Substring(0, getlength);
                }
            }

            TextBlock textBlock2 = new TextBlock();
            textBlock2.Text = min_y + "";
            textBlock2.Foreground = Brushes.Wheat;
            Canvas.SetLeft(textBlock2, 10);
            Canvas.SetTop(textBlock2, 240);
            canvas.Children.Add(textBlock2);

            TextBlock textBlock3 = new TextBlock();
            textBlock3.Text = max_y + "";
            textBlock3.Foreground = Brushes.Wheat;
            Canvas.SetLeft(textBlock3, 10);
            Canvas.SetTop(textBlock3, 40);
            canvas.Children.Add(textBlock3);

            TextBlock textBlock4 = new TextBlock();
            textBlock4.Text = Math.Round((max_y + min_y) / 2, 2) + "";
            textBlock4.Foreground = Brushes.Wheat;
            Canvas.SetLeft(textBlock4, 10);
            Canvas.SetTop(textBlock4, 140);
            canvas.Children.Add(textBlock4);

            Line Yline2 = new Line();
            Yline2.Stroke = Brushes.White;
            Yline2.StrokeThickness = 1.5;
            Yline2.X1 = 40;
            Yline2.Y1 = 270;
            Yline2.X2 = 40;
            Yline2.Y2 = 20;

            Line Xline2 = new Line();
            Xline2.Stroke = Brushes.White;
            Xline2.StrokeThickness = 1.5;
            Xline2.X1 = 40;
            Xline2.Y1 = 270;
            Xline2.X2 = 880;
            Xline2.Y2 = 270;
            canvas2.Children.Add(Yline2);
            canvas2.Children.Add(Xline2);

        }
        #endregion
        #region control_event
        public void ShowDifDea(object sender, MouseEventArgs e)
        {
            if (IsDes_DifDeaInfo_TextBox)
            {
                System.Action UpdateBubble = new System.Action(() =>
                {
                    canvas2.Children.Remove(DifDeaInfo_TextBox);
                    Line line = sender as Line;
                    DifDeaInfo_TextBox = new PUBubble();
                    DifDeaInfo_TextBox.Width = 150;
                    DifDeaInfo_TextBox.Height = 70;
                    MyPoint myPoint = line.DataContext as MyPoint;
                    string content = "日期：" + myPoint.date + "\n";
                    content += "DIF：" + Math.Round(myPoint.dif, 2) + "\n";
                    content += "DEA：" + Math.Round(myPoint.dea, 2);

                    DifDeaInfo_TextBox.Content = content;
                    //double y = Canvas.GetTop(ellipse) + ellipse.Height / 2;
                    //Info_TextBox.Content = ((Canvas.GetLeft(ellipse) + ellipse.Height / 2)-left_start_x)/cord_left + "," +(y+ max_y * (left_start_y / (min_y - max_y)))/ (left_start_y / (min_y - max_y));
                    canvas2.Children.Add(DifDeaInfo_TextBox);
                    Canvas.SetLeft(DifDeaInfo_TextBox, line.X2);
                    Canvas.SetTop(DifDeaInfo_TextBox, line.Y2 - 30);
                });
                this.canvas2.Dispatcher.Invoke(UpdateBubble);
                Thread.Sleep(20);
            }

        }
        public void HideDifDea(object sender, MouseEventArgs e)
        {
            System.Action RemoveBubble = new System.Action(() =>
            {
                canvas2.Children.Remove(DifDeaInfo_TextBox);
            });
            this.canvas2.Dispatcher.Invoke(RemoveBubble);
        }
        public void ShowVolume(object sender, MouseEventArgs e)
        {
            if (IsDes_DifDeaInfo_TextBox)
            {
                System.Action UpdateBubble = new System.Action(() =>
                {
                    canvas2.Children.Remove(DifDeaInfo_TextBox);
                    Rectangle line = sender as Rectangle;
                    DifDeaInfo_TextBox = new PUBubble();
                    DifDeaInfo_TextBox.Width = 150;
                    DifDeaInfo_TextBox.Height = 60;
                    MyPoint myPoint = line.DataContext as MyPoint;
                    string content = "日期：" + myPoint.date + "\n";
                    content += "成交量：" + myPoint.volume + "\n";

                    DifDeaInfo_TextBox.Content = content;
                    //double y = Canvas.GetTop(ellipse) + ellipse.Height / 2;
                    //Info_TextBox.Content = ((Canvas.GetLeft(ellipse) + ellipse.Height / 2)-left_start_x)/cord_left + "," +(y+ max_y * (left_start_y / (min_y - max_y)))/ (left_start_y / (min_y - max_y));
                    canvas2.Children.Add(DifDeaInfo_TextBox);
                    Canvas.SetLeft(DifDeaInfo_TextBox, Canvas.GetLeft(line) + 20);
                    Canvas.SetTop(DifDeaInfo_TextBox, Canvas.GetTop(line) + (250 - Canvas.GetTop(line)) / 4);
                });
                this.canvas2.Dispatcher.Invoke(UpdateBubble);
                Thread.Sleep(20);
            }

        }
        public void HideVolume(object sender, MouseEventArgs e)
        {
            System.Action RemoveBubble = new System.Action(() =>
            {
                canvas2.Children.Remove(DifDeaInfo_TextBox);
            });
            this.canvas2.Dispatcher.Invoke(RemoveBubble);
        }
        public void ShowPointInfo(object sender, MouseEventArgs e)
        {
            if (IsDes_Info_TextBox)
            {
                System.Action UpdateBubble = new System.Action(() =>
                {
                    canvas.Children.Remove(Info_TextBox);
                    Rectangle ellipse = sender as Rectangle;
                    Info_TextBox = new PUBubble();
                    Info_TextBox.Width = 150;
                    Info_TextBox.Height = 100;
                    MyPoint myPoint = ellipse.DataContext as MyPoint;
                    string content = "日期：" + myPoint.date + "\n";
                    content += "收盘价：" + Math.Round(myPoint.Y, 2) + "\n";
                    content += "开盘价：" + Math.Round(myPoint.openP, 2) + "\n";
                    content += "最高价：" + Math.Round(myPoint.maxP, 2) + "\n";
                    content += "最低价：" + Math.Round(myPoint.minP, 2);

                    Info_TextBox.Content = content;
                    //double y = Canvas.GetTop(ellipse) + ellipse.Height / 2;
                    //Info_TextBox.Content = ((Canvas.GetLeft(ellipse) + ellipse.Height / 2)-left_start_x)/cord_left + "," +(y+ max_y * (left_start_y / (min_y - max_y)))/ (left_start_y / (min_y - max_y));
                    canvas.Children.Add(Info_TextBox);
                    Canvas.SetLeft(Info_TextBox, Canvas.GetLeft(ellipse) + cord_width / (pointsum * 1.1));
                    Canvas.SetTop(Info_TextBox, Canvas.GetTop(ellipse) - ellipse.Height);
                });
                this.canvas.Dispatcher.Invoke(UpdateBubble);
                Thread.Sleep(20);
            }
        }
        public void HidePointInfo(object sender, MouseEventArgs e)
        {
            System.Action RemoveBubble = new System.Action(() =>
            {
                Ellipse ellipse = sender as Ellipse;
                canvas.Children.Remove(Info_TextBox);
                IsDes_Info_TextBox = true;
            });
            this.canvas.Dispatcher.Invoke(RemoveBubble);
            Thread.Sleep(20);
        }
        #endregion
        #region nouse
        //public void ShowPointInfo(object sender, MouseEventArgs e)
        //{
        //    if (IsDes_Info_TextBox)
        //    {
        //        Action UpdateBubble = new Action(() =>
        //        {
        //            Ellipse ellipse = sender as Ellipse;
        //            Point positionToTite = e.GetPosition(ellipse);
        //            if (Math.Sqrt((positionToTite.X-ellipse.ActualWidth/2)* (positionToTite.X - ellipse.ActualWidth / 2)+ (positionToTite.Y - ellipse.ActualHeight / 2)* (positionToTite.Y - ellipse.ActualHeight / 2))<ellipse.ActualWidth/2)
        //            {
        //                Info_TextBox = new PUBubble();
        //                Info_TextBox.Width = 80;
        //                Info_TextBox.Height = 30;
        //                Info_TextBox.Content = (Canvas.GetLeft(ellipse)+ellipse.Height/2) + "," + (Canvas.GetTop(ellipse) + ellipse.Height / 2);
        //                canvas.Children.Add(Info_TextBox);
        //                Canvas.SetLeft(Info_TextBox, Canvas.GetLeft(ellipse) + ellipse.Height);
        //                Canvas.SetTop(Info_TextBox, Canvas.GetTop(ellipse)-ellipse.Height);
        //                IsDes_Info_TextBox = false;
        //            }
        //        });
        //        this.canvas.Dispatcher.Invoke(UpdateBubble);
        //        Thread.Sleep(20);
        //    }
        //}


        //public void HidePointInfo(object sender, MouseEventArgs e)
        //{
        //    if (IsDes_Info_TextBox == false)
        //    {
        //        Action RemoveBubble = new Action(() =>
        //        {
        //            Ellipse ellipse = sender as Ellipse;
        //            Point positionToTite = e.GetPosition(ellipse);
        //            if (Math.Sqrt((positionToTite.X - ellipse.ActualWidth / 2) * (positionToTite.X - ellipse.ActualWidth / 2) + (positionToTite.Y - ellipse.ActualHeight / 2) * (positionToTite.Y - ellipse.ActualHeight / 2)) >= ellipse.ActualWidth/2)
        //            {
        //                canvas.Children.Remove(Info_TextBox);
        //                IsDes_Info_TextBox = true;
        //            }
        //        });
        //        this.canvas.Dispatcher.Invoke(RemoveBubble);
        //        Thread.Sleep(20);
        //    }
        //}


        #endregion
        #region 更新table操作
        /// <summary>
        /// 查询点击操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetDataAndPaint(object sender, RoutedEventArgs e)
        {
            try
            {
                startDate = Convert.ToDateTime(startTime.Text);
                endDate = Convert.ToDateTime(endTime.Text);
                string stockid = StockId.Text;
                Paint(startDate, endDate, stockid);
                canvas.MouseWheel += WheelUpdateTable;
                isdataInTable = true;
            }
            catch (Exception)
            {
                MessageBox.Show("未输入时间");
            }

        }
        /// <summary>
        /// 绘制table
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="stockid"></param>
        public void Paint(DateTime startDate, DateTime endDate, string stockid)
        {
            try
            {
                canvas.Children.Clear();
                canvas2.Children.Clear();
                List<MyPoint> points = GetData(startDate, endDate, stockid);
                //if (points.Count() < 5)
                //{
                //    MessageBox.Show("数据过少");
                //    return;
                //}
                decimal max = int.MinValue;
                decimal min = int.MaxValue;
                for (int i = 0; i < points.Count(); i++)
                {
                    if (points[i].Y > max)
                    {
                        max = points[i].Y;
                    }
                    if (points[i].Y < min)
                    {
                        min = points[i].Y;
                    }
                }
                max_y = max;
                min_y = min;
                k = cord_height / (min_y - max_y);
                b = -max_y * (cord_height / (min_y - max_y));

                double line_right_pianyi = cord_width / (points.Count() * 1.1) / 2;
                for (int i = 0; i < points.Count(); i++)
                {
                    InfoRec(points[i], (double)(i + 1) / (double)(points.Count() + 1) * cord_width, r, points.Count());
                }
                pointsum = points.Count();
                DrawCord(points);
                //绘制均线
                max = int.MinValue;
                min = int.MaxValue;
                for (int i = 0; i < points.Count(); i++)
                {
                    if (points[i].ma5 > max)
                    {
                        max = points[i].ma5;
                    }
                    if (points[i].ma10 > max)
                    {
                        max = points[i].ma10;
                    }
                    if (points[i].ma20 > max)
                    {
                        max = points[i].ma20;
                    }
                    if (points[i].ma30 > max)
                    {
                        max = points[i].ma30;
                    }
                    if (points[i].ma60 > max)
                    {
                        max = points[i].ma60;
                    }

                    if (points[i].ma5 < min)
                    {
                        min = points[i].ma5;
                    }
                    if (points[i].ma10 < min)
                    {
                        min = points[i].ma10;
                    }
                    if (points[i].ma20 < min)
                    {
                        min = points[i].ma20;
                    }
                    if (points[i].ma30 < min)
                    {
                        min = points[i].ma30;
                    }
                    if (points[i].ma60 < min)
                    {
                        min = points[i].ma60;
                    }
                }
                max_y = max;
                min_y = min;
                k = cord_height / (min_y - max_y);
                b = -max_y * (cord_height / (min_y - max_y));

                line_right_pianyi = cord_width / (points.Count() * 1.1) / 2;

                for (int i = 0; i < points.Count(); i++)
                {
                    //InfoRec(points[i], (double)(i + 1) / (double)(points.Count() + 1) * cord_width, r, points.Count());

                    if (i > 0)
                    {
                        //ma5线，白色
                        AddLine(left_start_x, left_start_y, (double)(i) / (double)(points.Count() + 1) * cord_width, points[i - 1].ma5, (double)(i + 1) / (double)(points.Count() + 1) * cord_width, points[i].ma5, r, Brushes.White, line_right_pianyi);
                        //ma10线，黄色
                        AddLine(left_start_x, left_start_y, (double)(i) / (double)(points.Count() + 1) * cord_width, points[i - 1].ma10, (double)(i + 1) / (double)(points.Count() + 1) * cord_width, points[i].ma10, r, Brushes.Yellow, line_right_pianyi);
                        //ma20线，粉色
                        AddLine(left_start_x, left_start_y, (double)(i) / (double)(points.Count() + 1) * cord_width, points[i - 1].ma20, (double)(i + 1) / (double)(points.Count() + 1) * cord_width, points[i].ma20, r, Brushes.Pink, line_right_pianyi);
                        //ma30线，绿色
                        AddLine(left_start_x, left_start_y, (double)(i) / (double)(points.Count() + 1) * cord_width, points[i - 1].ma30, (double)(i + 1) / (double)(points.Count() + 1) * cord_width, points[i].ma30, r, Brushes.Green, line_right_pianyi);
                        //ma60线，蓝色
                        AddLine(left_start_x, left_start_y, (double)(i) / (double)(points.Count() + 1) * cord_width, points[i - 1].ma60, (double)(i + 1) / (double)(points.Count() + 1) * cord_width, points[i].ma60, r, Brushes.Blue, line_right_pianyi);
                    }
                }

                //绘制macd图像
                if (MACDorVolume == 0)
                {
                    max = int.MinValue;
                    min = int.MaxValue;
                    for (int i = 0; i < points.Count(); i++)
                    {
                        if (points[i].dif > max)
                        {
                            max = points[i].dif;
                        }
                        if (points[i].dif < min)
                        {
                            min = points[i].dif;
                        }
                    }
                    max_y = max;
                    min_y = min;
                    k2 = cord_height / (min_y - max_y);
                    b2 = -max_y * (cord_height / (min_y - max_y));

                    //double line_right_pianyi = cord_width / (points.Count() * 1.1) / 2;
                    for (int i = 0; i < points.Count(); i++)
                    {
                        //InfoRec(points[i], (double)(i + 1) / (double)(points.Count() + 1) * cord_width, r, points.Count());

                        if (i > 0)
                        {
                            //dif线，灰色
                            AddMACDLine(points[i], left_start_x, left_start_y, (double)(i) / (double)(points.Count() + 1) * cord_width, points[i - 1].dif, (double)(i + 1) / (double)(points.Count() + 1) * cord_width, points[i].dif, r, Brushes.Gray);

                        }
                    }

                    max = int.MinValue;
                    min = int.MaxValue;
                    for (int i = 0; i < points.Count(); i++)
                    {
                        if (points[i].dea > max)
                        {
                            max = points[i].dea;
                        }
                        if (points[i].dea < min)
                        {
                            min = points[i].dea;
                        }
                    }
                    max_y = max;
                    min_y = min;
                    k2 = cord_height / (min_y - max_y);
                    b2 = -max_y * (cord_height / (min_y - max_y));

                    //double line_right_pianyi = cord_width / (points.Count() * 1.1) / 2;
                    for (int i = 0; i < points.Count(); i++)
                    {
                        //InfoRec(points[i], (double)(i + 1) / (double)(points.Count() + 1) * cord_width, r, points.Count());

                        if (i > 0)
                        {
                            //dea线，黄色
                            AddMACDLine(points[i], left_start_x, left_start_y, (double)(i) / (double)(points.Count() + 1) * cord_width, points[i - 1].dea, (double)(i + 1) / (double)(points.Count() + 1) * cord_width, points[i].dea, r, Brushes.Yellow);

                        }
                    }
                }
                //绘制成交量柱形图
                else if (MACDorVolume == 1)
                {
                    long l_max = long.MinValue;
                    long l_min = long.MaxValue;
                    for (int i = 0; i < points.Count(); i++)
                    {
                        if (points[i].volume > l_max)
                        {
                            l_max = points[i].volume;
                        }
                        if (points[i].volume < l_min)
                        {
                            l_min = points[i].volume;
                        }
                    }
                    max_y = l_max;
                    min_y = l_min;
                    k2 = cord_height / (min_y - max_y);
                    b2 = -max_y * (cord_height / (min_y - max_y));

                    //double line_right_pianyi = cord_width / (points.Count() * 1.1) / 2;
                    for (int i = 0; i < points.Count(); i++)
                    {

                        //成交量柱形图
                        AddVolumeRec(points[i], left_start_x, left_start_y, (double)(i + 1) / (double)(points.Count() + 1) * cord_width, points[i].volume, r, Brushes.Yellow);

                    }
                }

            }
            catch (Exception e)
            {

            }

        }
        /// <summary>
        /// 获取表中数据
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="stockid"></param>
        /// <returns></returns>
        public List<MyPoint> GetData(DateTime startDate, DateTime endDate, string stockid)
        {
            List<MyPoint> points = new List<MyPoint>();
            var list = db.StockInfo.Where(t => t.date >= startDate & t.date <= endDate & t.id == stockid).OrderBy(t => t.date).ToList();
            for (int i = 0; i < list.Count(); i++)
            {
                MyPoint myPoint = new MyPoint()
                {
                    X = (i + 1) / cord_width,
                    Y = list[i].closeprice.Value,
                    date = list[i].date.ToString("yyyy-MM-dd"),
                    closeP = list[i].closeprice.Value,
                    openP = list[i].openprice.Value,
                    maxP = list[i].maxprice.Value,
                    minP = list[i].minprice.Value,
                    ma5 = list[i].MA5.Value,
                    ma10 = list[i].MA10.Value,
                    ma20 = list[i].MA20.Value,
                    ma30 = list[i].MA30.Value,
                    ma60 = list[i].MA60.Value,
                    dif = list[i].DIF.Value,
                    dea = list[i].DEA.Value,
                    volume = list[i].volume.Value,
                    InOutP = list[i].InOutPoint
                };
                //if(list[i].IsMACD == 0)
                //{
                //    myPoint.color = "#FF000000";
                //}
                //else if(list[i].IsMACD == 1)
                //{
                //    myPoint.color = "#FF00FF00";
                //}
                //else if (list[i].IsMACD == 2)
                //{
                //    myPoint.color = "#FF0000FF";
                //}
                myPoint.color = "#FF000000";
                if (list[i].InOutPoint == 1)
                {
                    myPoint.color = "#FFFFFFFF";
                }

                if (myPoint.Y != 0) points.Add(myPoint);

            }
            return points;
        }

        private void transToDate(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string s_date = button.DataContext as string;
            DateTime date = DateTime.Parse(s_date);
            DateTime startDate = date.AddDays(-10);
            DateTime endDate = date.AddDays(20);
            string stockid = StockId.Text;
            Paint(startDate, endDate, stockid);
            //MessageBox.Show(elicolor.SelectedValue.ToString());
        }

        public void GetMore()
        {
            System.Action udIsUpdateTable = () =>
            {
                isUodateTable = true;
            };
            this.Dispatcher.BeginInvoke(udIsUpdateTable);
            Thread.Sleep(50);
            startDate = startDate.AddDays(-1);
            endDate = endDate.AddDays(1);
            string stockid = StockId.Text;
            var stockinfo = db.StockInfo.FirstOrDefault(t => (t.date == startDate || t.date == endDate) && t.id == stockid);
            if (stockinfo == null)
            {
                GetMore();
            }
            else
            {
                Paint(startDate, endDate, stockid);
            }
            System.Action udIsUpdateTable2 = () =>
            {
                isUodateTable = false;
            };
            this.Dispatcher.BeginInvoke(udIsUpdateTable2);
            Thread.Sleep(50);
        }
        public void GetLess()
        {
            System.Action udIsUpdateTable = () =>
            {
                isUodateTable = true;
            };
            this.Dispatcher.BeginInvoke(udIsUpdateTable);
            Thread.Sleep(50);
            startDate = startDate.AddDays(1);
            endDate = endDate.AddDays(-1);
            string stockid = StockId.Text;
            var stockinfo = db.StockInfo.FirstOrDefault(t => (t.date == startDate || t.date == endDate) && t.id == stockid);
            if (stockinfo == null)
            {
                GetLess();
            }
            else
            {
                Paint(startDate, endDate, stockid);
            }
            System.Action udIsUpdateTable2 = () =>
            {
                isUodateTable = false;
            };
            this.Dispatcher.BeginInvoke(udIsUpdateTable2);
            Thread.Sleep(50);
        }
        public void GetLeft()
        {
            System.Action udIsUpdateTable = () =>
            {
                isUodateTable = true;
            };
            this.Dispatcher.BeginInvoke(udIsUpdateTable);
            Thread.Sleep(50);
            startDate = startDate.AddDays(-1);
            endDate = endDate.AddDays(-1);
            string stockid = StockId.Text;
            var stockinfo = db.StockInfo.FirstOrDefault(t => (t.date == startDate || t.date == endDate) && t.id == stockid);
            if (stockinfo == null)
            {
                GetLeft();
            }
            else
            {
                Paint(startDate, endDate, stockid);
            }
            System.Action udIsUpdateTable2 = () =>
            {
                isUodateTable = false;
            };
            this.Dispatcher.BeginInvoke(udIsUpdateTable2);
            Thread.Sleep(50);
        }
        public void GetRight()
        {
            System.Action udIsUpdateTable = () =>
            {
                isUodateTable = true;
            };
            this.Dispatcher.BeginInvoke(udIsUpdateTable);
            Thread.Sleep(50);
            startDate = startDate.AddDays(1);
            endDate = endDate.AddDays(1);
            string stockid = StockId.Text;
            var stockinfo = db.StockInfo.FirstOrDefault(t => (t.date == startDate || t.date == endDate) && t.id == stockid);
            if (stockinfo == null)
            {
                GetRight();
            }
            else
            {
                Paint(startDate, endDate, stockid);
            }
            System.Action udIsUpdateTable2 = () =>
            {
                isUodateTable = false;
            };
            this.Dispatcher.BeginInvoke(udIsUpdateTable2);
            Thread.Sleep(50);
        }
        private void WheelUpdateTable(object sender, MouseWheelEventArgs e)
        {
            if (!isUodateTable)
            {
                if (e.Delta > 0)
                {
                    GetMore();
                }
                else
                {
                    GetLess();
                }
            }

        }
        private void LeftMoveTable(object sender, RoutedEventArgs e)
        {
            if (!isUodateTable & isdataInTable)
            {
                GetLeft();
            }
        }
        private void RightMoveTable(object sender, RoutedEventArgs e)
        {
            if (!isUodateTable & isdataInTable)
            {
                GetRight();
            }
        }

        private void Table2Change(object sender, SelectionChangedEventArgs e)
        {

            PUComboBox control = sender as PUComboBox;
            PUComboBoxItem a = control.SelectedItem as PUComboBoxItem;
            MACDorVolume = int.Parse(a.Value.ToString());

            if (!isUodateTable & isdataInTable)
            {
                string stockid = StockId.Text;
                Paint(startDate, endDate, stockid);
            }
        }





        #endregion

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            // 激活的是当前默认的浏览器
            Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
            Clipboard.SetText(link.NavigateUri.AbsoluteUri);
        }

        public void TransToDate(object sender, RoutedEventArgs e)
        {
            PUButton pUButton = sender as PUButton;
            Model.HotStock hotStock = pUButton.DataContext as HotStock;
            startDate = hotStock.date.AddDays(-20);
            endDate = hotStock.date.AddDays(5);
            StockId.Text = hotStock.id;
            Paint(startDate, endDate, hotStock.id);
            canvas.MouseWheel += WheelUpdateTable;
            isdataInTable = true;
        }
    }
}
