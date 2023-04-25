using System;
using System.Reflection;
using Devart.Common;

namespace DevartKeyGen {
  internal class Program {
    public static void Main(string[] args) {
      var license = new LicenseInfo() {
        Company = "Devart",
        EndDate = DateTime.Now.AddYears(1),
        UserCount = 99,
        UserName = "Devart user",
        Note = "Sergiye.3xtraTools",
        LicenseType = LicenseType.Paid,
        ProductName = "Code Compare Pro",
        Signature = new byte[] { 143, 128, 95, 196, 125, 43, 136, 114, 197, 223, 109, 202, 243, 119, 20, 54 },
        LicenseNumber = "1234567890"
      };
      var str = license.ToString();
      Console.WriteLine(str);
      // R9sv+a4wxAVZ/itGbmvBx1xWMsAixQ8wYDeX0dMo93cIOLG5MsR6NGp7PusiNHr1
      // Mq4kS3Mut2V2Y/oDAq5umVaUz1vK+V/Td8tKIYQWSe7Tn1soAhJsvBEXJXXOMVBo
      // gOKVzbuYaqDl3OFM/7qLmw==

      license = LicenseInfo.Parse(str, Assembly.GetExecutingAssembly());

      Console.WriteLine("Press 'Enter' to exit...");
      Console.ReadLine();
    }
  }
}