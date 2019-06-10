using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
//When use database to fetch products
//using MySql.Data.MySqlClient;
//When use file to load products
//using System.IO;


namespace GroceryStore
{
    class Program
    {
        static void Main(string[] args)
        {
            ItemPacking ip = new ItemPacking();
            ip.LoadProductInfo();
            //SystemTest(ip);
            UserInteraction(ip);
        }

        static void SystemTest(ItemPacking ip)
        {
            Console.WriteLine("Start Testing...");
            int totalNum = 0;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (string productCode in ip.productList.Keys)
            {
                //Special Cases
                ip.ResponseToInput(-1, productCode, true);
                ip.ResponseToInput(0, productCode, true);
                ip.ResponseToInput(1, productCode, true);
                totalNum += 3;
                for (int i = 2; i <= 80; i++)
                {
                    //Only display successful cases
                    ip.ResponseToInput(i, productCode, false);
                    totalNum++;
                }
            }
            stopwatch.Stop();
            TimeSpan timespan = stopwatch.Elapsed;
            double seconds = timespan.TotalSeconds;
            Console.WriteLine("Tested {0} cases, using {1} seconds.",totalNum,seconds);
            Console.WriteLine("");
        }

        static void UserInteraction(ItemPacking ip)
        {
            while (true)
            {
                Console.WriteLine("Now you can input to test any other cases...");
                Console.WriteLine("Please input the number you want for the product and the product code:");
                string[] input = Console.ReadLine().Split(' ');
                if (input.Length != 2)
                {
                    Console.WriteLine("Your input format should be \'<Product Number> <Product Code>\'. E.g. \'10 SH3\'.");
                    continue;
                }
                bool isInputDigit = int.TryParse(input[0], out int number);
                if (!isInputDigit)
                {
                    Console.WriteLine("Your input format should be \'<Product Number> <Product Code>\'. E.g. \'10 SH3\'.");
                    continue;
                }
                ip.ResponseToInput(number, input[1], true);
            }
        }

    }

    class ItemPacking
    {
        public Dictionary<string, Product> productList = new Dictionary<string, Product>();

        public void LoadProductInfo()
        {
            //Can load from database
            #region LoadFromDatabase
            /*
            MySqlConnection m_conn = new MySqlConnection("server=localhost;user id=root;password=root;database=adatabase");
            try
            {
                m_conn.Open();
                string sql = "select * from product join packinfo order by product_id, pack";
                MySqlCommand cmd = new MySqlCommand(sql, m_conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                string productName = String.Empty;
                Product newProduct = null;
                while (reader.Read())
                {
                    if (!productName.Equals(reader.GetString("productName")))
                    {
                        productName = reader.GetString("productName");
                        newProduct = new Product(productName, reader.GetString("productCode"));
                        this.productList.Add(reader.GetString("productCode"),newProduct);
                    }
                    newProduct.AddPackInfo(reader.GetInt32("packNo"), reader.GetFloat('price'));
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                m_conn.Close();
            }
            */
            #endregion
            //Or load from file(.txt/.csv)
            #region LoadFromFile
            /*
            using (StreamReader sr = new StreamReader("fileName.csv",true))
            {
                //Read Head: name,code,pack,price
                sr.ReadLine();
                string productName = String.Empty;
                Product newProduct = null;
                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    if (!productName.Equals(rows[0]))
                    {
                        productName = rows[0];
                        newProduct = new Product(productName, row[1]);
                        this.productList.Add( row[1],newProduct);
                    }
                    newProduct.AddPackInfo(int.Parse(rows[2]), float.Parse(rows[3]));
                }
            }
            */
            #endregion
            //Or just input it
            #region LoadProduct
            Product newProduct = new Product("Sliced Ham", "SH3");
            this.productList.Add("SH3", newProduct);
            newProduct.AddPackInfo(3, (float)(2.99));
            newProduct.AddPackInfo(5, (float)(4.49));
            newProduct = new Product("Yoghurt", "YT2");
            this.productList.Add("YT2", newProduct);
            newProduct.AddPackInfo(4, (float)(4.95));
            newProduct.AddPackInfo(10, (float)(9.95));
            newProduct.AddPackInfo(15, (float)(13.95));
            newProduct = new Product("Toilet Rolls", "TR");
            this.productList.Add("TR", newProduct);
            newProduct.AddPackInfo(3, (float)(2.95));
            newProduct.AddPackInfo(5, (float)(4.45));
            newProduct.AddPackInfo(9, (float)(7.99));
            #endregion
        }

        public void ResponseToInput(int number, string productCode, bool printFailure)
        {
            if (number <= 0)
            {
                if (printFailure)
                {
                    Console.WriteLine("Please input a number larger than 0.");
                }
                return;
            }
            if (!this.productList.ContainsKey(productCode))
            {
                if (printFailure)
                {
                    Console.WriteLine("Please input a valid product code.");
                }
                return;
            }
            Product selectProduct = this.productList[productCode];
            int[] method = this.PackDivide(number, selectProduct);
            if (this.IsZeroArray(method))
            {
                if (printFailure)
                {
                    Console.WriteLine("The number you want can not be packed.");
                }
                return;
            }
            else
            {
                Console.Write("{0} {1}", number, productCode);
                int[] packInfo = selectProduct.GetPackInfo();
                float[] priceInfo = selectProduct.GetPriceInfo();
                float totalPrice = 0;
                for (int i = 0; i < packInfo.Length; i++)
                {
                    totalPrice += method[i] * priceInfo[i];
                }
                Console.WriteLine(" ${0}", totalPrice);
                for (int i = 0; i < packInfo.Length; i++)
                {
                    if (method[i] != 0)
                    {
                        Console.Write("  ");
                        Console.Write(method[i]);
                        Console.Write(" * ");
                        Console.Write(packInfo[i]);
                        Console.Write(" $");
                        Console.WriteLine(method[i] * priceInfo[i]);
                    }
                }
            }
        }

        public bool IsZeroArray(int[] tmpArray)
        {
            bool equalZero = true;
            if(tmpArray == null)
            {
                return true;
            }
            foreach (int a in tmpArray)
            {
                if (a != 0)
                {
                    equalZero = false;
                }
            }
            return equalZero;
        }

        public int[] PackDivide(int number, Product product)
        {
            int[] packInfo = product.GetPackInfo();
            int types = packInfo.Count();
            if (types == 0 || number < 0)
            {
                return null;
            }
            if (number == 0)
            {
                int[] zeroArray = new int[types];
                product.AddPackMethod(0, zeroArray);
                return zeroArray;
            }
            else
            {
                if (product.PackMethodHasKey(number))
                {
                    //if dict has, use it
                    return product.GetPackMethod(number);
                }
                else
                {
                    //Record the previous method when haven't added new pack
                    int[] bestMethod = new int[types];
                    int minPack = Int32.MaxValue;
                    int bestIndex = -1;
                    int zeroCount = 0;
                    //Try all the packs, see which one causes a min total packages
                    for (int i = 0; i < packInfo.Length; i++)
                    {
                        int packNo = packInfo[i];
                        if (number - packNo >= 0)
                        {
                            int[] tmpMethod = PackDivide(number - packNo, product);
                            //Try all the time and All get the zero means no package or can not be packed.
                            if (this.IsZeroArray(tmpMethod) && number - packNo > 0)
                            {
                                zeroCount++;
                                continue;
                            }
                            //current pack number when new pack is added
                            int tmpValue = tmpMethod.Sum() + 1;
                            if (tmpValue <= minPack)
                            {
                                tmpMethod.CopyTo(bestMethod, 0);
                                minPack = tmpValue;
                                bestIndex = i;
                            }
                        }
                    }
                    //Try all packs, previous number still unable to pack
                    if (zeroCount == types)
                    {
                        int[] zeroArray = new int[types];
                        product.AddPackMethod(number, zeroArray);
                        return zeroArray;
                    }
                    //has some method
                    if (bestIndex != -1)
                    {
                        bestMethod[bestIndex] += 1;
                    }
                    //add to dict, for next usage, no calculate again
                    product.AddPackMethod(number, bestMethod);
                    return bestMethod;
                }
            }
        }
    }

    class Product
    {
        //Product name,code
        private string name;
        private string code;
        //Item no. in pack, pack price, e.g.  {3:2.99,5:4.49,....}
        private Dictionary<int, float> packPrice;
        //item no. pack method, e.g. {0:[0,0],3:[1,0],...}
        private Dictionary<int, int[]> packMethod;

        public Product(string name, string code)
        {
            this.name = name;
            this.code = code;
            this.packPrice = new Dictionary<int, float>();
            this.packMethod = new Dictionary<int, int[]>();
        }
        #region Getter&Setter
        public string GetName()
        {
            return this.name;
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public string GetCode()
        {
            return this.code;
        }

        public void SetCode(string code)
        {
            this.code = code;
        }

        public void AddPackInfo(int number, float price)
        {
            this.packPrice.Add(number, price);
        }

        public int[] GetPackInfo()
        {
            return this.packPrice.Keys.ToArray();
        }

        public float[] GetPriceInfo()
        {
            return this.packPrice.Values.ToArray();
        }

        public Dictionary<int, float> GetPackPrice()
        {
            return this.packPrice;
        }

        public void AddPackMethod(int number, int[] method)
        {
            if (!this.PackMethodHasKey(number))
            {
                this.packMethod.Add(number, method);
            }
        }

        public int[] GetPackMethod(int number)
        {
            return this.packMethod.ContainsKey(number) ? this.packMethod[number] : null;
        }

        public bool PackMethodHasKey(int number)
        {
            return this.packMethod.ContainsKey(number);
        }
        #endregion
    }
}
