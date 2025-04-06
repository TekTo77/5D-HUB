

using Interfase;

namespace Result
{
    public class UseridClass :  InterfaceId
    {
        

        public int Userid {  set; get; }
    }

    public class RegistrResult : UseridClass, InterfaceId
    {
        
        public bool Flag { get; set; }

       public required string Token { get; set; }



    }



}