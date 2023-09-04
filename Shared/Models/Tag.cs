using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeRecorder.Shared
{
    public class Tag
    {
        //public int id;
        public string description { get; set; }

        public Tag (string description = "")
        {
            this.description = description;
        }
    }
}
