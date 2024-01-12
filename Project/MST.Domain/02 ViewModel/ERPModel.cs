using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST.Domain.ViewModel
{
    public class ERPModel
    {
        public string Timetamp { get; set; }
        public DateTime? DateTime { get; set; }
        public string MsgID { get; set; }
        public string Func { get; set; }
        public object Param { get; set; }
    }
}
