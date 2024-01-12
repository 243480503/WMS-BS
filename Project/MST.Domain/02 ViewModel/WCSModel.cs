using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST.Domain.ViewModel
{
    public class WCSModel
    {
        public string MsgID { get; set; }
        public DateTime? AskTime { get; set; }
        public string Method { get; set; }
        public object PostData { get; set; }
    }
}
