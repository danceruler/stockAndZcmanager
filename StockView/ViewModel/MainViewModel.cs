using Stock;
using StockView.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StockView.ViewModel
{
    public class MainViewModel: ViewModelBase
    {
        private MainWindow thisWindow;
        private StockSupportSystemEntities db = new StockSupportSystemEntities();
        public MainViewModel(MainWindow window)
        {
            thisWindow = window;
        }

        public List<List_MACDModel> List_MACDModels
        {
            get { return _list_MACDModels; }
            set { _list_MACDModels = value; RaisePropertyChanged("List_MACDModels");}
        }
        private List<List_MACDModel> _list_MACDModels;

        //public ICommand UpdateMACDListCommand
        //{
        //    get{ return new QueryCommand(UpdateMACDList); }
        //}

        //public void UpdateMACDList()
        //{
        //    if(thisWindow.elicolor.SelectedValue.ToString().Trim().Split(':')[1].Trim() == "全部")
        //    {
        //        string stockid = thisWindow.StockId.Text;
        //        var MACD_List = db.StockInfo.Where(t => t.id == stockid & t.InOutPoint == 1).ToList();
        //        List<List_MACDModel> temp_list = new List<List_MACDModel>();
        //        for (int i = 0; i < MACD_List.Count(); i++)
        //        {
        //            temp_list.Add(new List_MACDModel()
        //            {
        //                Title = MACD_List[i].date.ToString("yyyy-MM-dd") + "," + MACD_List[i].IsMACD,
        //                Date = MACD_List[i].date.ToString("yyyy-MM-dd"),
        //            });
        //        }
        //        List_MACDModels = temp_list;
        //    }else if (thisWindow.elicolor.SelectedValue.ToString().Trim().Split(':')[1].Trim() == "蓝")
        //    {
        //        string stockid = thisWindow.StockId.Text;
        //        var MACD_List = db.StockInfo.Where(t => t.id == stockid &t.IsMACD == 2).ToList();
        //        List<List_MACDModel> temp_list = new List<List_MACDModel>();
        //        for (int i = 0; i < MACD_List.Count(); i++)
        //        {
        //            temp_list.Add(new List_MACDModel()
        //            {
        //                Title = MACD_List[i].date.ToString("yyyy-MM-dd") + "," + MACD_List[i].IsMACD,
        //                Date = MACD_List[i].date.ToString("yyyy-MM-dd"),
        //            });
        //        }
        //        List_MACDModels = temp_list;
        //    }
        //    else
        //    {
        //        string stockid = thisWindow.StockId.Text;
        //        var MACD_List = db.StockInfo.Where(t => t.id == stockid &t.IsMACD == 1).ToList();
        //        List<List_MACDModel> temp_list = new List<List_MACDModel>();
        //        for (int i = 0; i < MACD_List.Count(); i++)
        //        {
        //            temp_list.Add(new List_MACDModel()
        //            {
        //                Title = MACD_List[i].date.ToString("yyyy-MM-dd") + "," + MACD_List[i].IsMACD,
        //                Date = MACD_List[i].date.ToString("yyyy-MM-dd"),
        //            });
        //        }
        //        List_MACDModels = temp_list;
        //    }

        //}
    }
}
