namespace WebServerMVCv2.Models
{
    public class HomeModel
    {
        public Dictionary<int, string> _idTitleDictionary = new Dictionary<int, string>();
        public int rowCalculation = 0;


        public void SetRowCalculation()
        {
            if(_idTitleDictionary != null && _idTitleDictionary.Count > 0)
            {
                rowCalculation = (_idTitleDictionary.Count / 3) + 1;
            }
            else
            {
                rowCalculation = 0;
            }
        }


    }


}
