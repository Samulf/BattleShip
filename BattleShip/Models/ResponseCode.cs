using System;
using System.Collections.Generic;
using System.Text;

namespace BattleShipServer
{
    public class ResponseCode
    {
        public string Code        { get; set; }
        public string FullString  { get; set; }

        public ResponseCode()
        {

        }
        public ResponseCode(string code)
        {
            Code = code;
        }

        public ResponseCode(string code, string full)
        {
            Code = code;
            FullString = full;
        }
    }
}
