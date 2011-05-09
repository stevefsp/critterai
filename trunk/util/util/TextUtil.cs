using System;
using System.Text;

namespace org.critterai
{
    public static class TextUtil
    {

        public static string GetCloneName(string origName)
        {
            if (origName.Length < 3)
                return origName + "01";

            string suffix = origName.Substring(origName.Length - 2, 2);

            int result;
            if (Int32.TryParse(suffix, out result))
                result += 1;
            else
                return origName + "01"; 

            return origName.Substring(0, origName.Length - 2) 
                + result.ToString("00");
        }
    }
}
