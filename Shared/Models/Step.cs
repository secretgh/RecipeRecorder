using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeRecorder.Shared
{
    public class Step
    {
        public int id { get; set; }
        public string description { get; set; }

        public Step(int id = -1,string description = "")
        {
            this.id = id;
            this.description = description;
        }
    }
}
