using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    public class BaseMessage
    {
        public string Action { get; set; }
        public JObject Data { get; set; }
    }


