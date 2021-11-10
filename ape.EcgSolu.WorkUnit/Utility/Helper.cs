using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ape.EcgSolu.WorkUnit
{
    class Helper
    {
        /// <summary>
        /// 性别转换映射
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GenderConvert(int value)
        {
            string gender = string.Empty;
            switch (value)
            {
                case -1:
                    gender = "未知";
                    break;
                case 0:
                    gender = "女";
                    break;
                case 1:
                    gender = "男";
                    break;
            }
            return gender;
        }
    }
}
