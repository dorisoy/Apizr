﻿using Refit;

namespace Apizr.Sample.Api.Models
{
    public class ReadAllUsersParams
    {
        public ReadAllUsersParams()
        {
            
        }

        public ReadAllUsersParams(string param1, int param2)
        {
            Param1 = param1;
            Param2 = param2;
        }

        [AliasAs("p1")]
        public string Param1 { get; set; }

        [AliasAs("p2")]
        public int? Param2 { get; set; }
    }
}