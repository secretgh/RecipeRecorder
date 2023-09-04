using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeRecorder.Shared
{
    public class Ingredient
    {
        //public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public float amount { get; set; }
        public string amountDescription { get; set; }

        public Ingredient(string name = "", string description = "", float amount = 0, string amountDesc = "")
        {
            this.name = name;
            this.description = description;
            this.amount = amount;
            this.amountDescription = amountDesc;
        }
    }
}
