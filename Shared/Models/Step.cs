using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeRecorder.Shared
{
    public class Step
    {
        //public int id;
        public string description { get; set; }

        public Step(string description = "")
        {
            this.description = description;
        }
    }
}
