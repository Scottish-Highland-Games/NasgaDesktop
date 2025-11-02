using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MainViewModel : BaseViewModel
{
    private string _message;
    public string HomeTabMessage
    {
        get { return _message; }
        set { _message = value; OnPropertyChanged(); }
    }

    public MainViewModel()
    {
        HomeTabMessage = "Hello, MVVM!";
    }
}