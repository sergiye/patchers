using System;
using System.IO;
using System.Text.RegularExpressions;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace codeComparePatcher {

  internal class Program {

    private static ModuleContext mainCtx;
    private static ModuleDefMD main;

    private static void PrintColor(string message, ConsoleColor color) {
      var pieces = Regex.Split(message, @"(\[[^\]]*\])");

      for (var i = 0; i < pieces.Length; i++) {
        var piece = pieces[i];

        if (piece.StartsWith("[") && piece.EndsWith("]")) {
          Console.ForegroundColor = color;
          piece = piece.Substring(1, piece.Length - 2);
        }

        Console.Write(piece);
        Console.ResetColor();
      }

      Console.WriteLine();
    }

    private static void Print(String data) {
      Console.WriteLine(data);
    }

    private static TypeDef GetType(ModuleDefMD module, string classPath) {
      foreach (var type in module.Types) {
        if (type.FullName == classPath)
          return type;
      }

      return null;
    }
    
    private static CilBody return_bool(bool status) {
      var body = new CilBody();
      var state = status ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
      body.Instructions.Add(state.ToInstruction());
      body.Instructions.Add(OpCodes.Ret.ToInstruction());
      return body;
    }
    
    private static CilBody return_digit(int num) {
      var body = new CilBody();
      body.Instructions.Add(OpCodes.Ldc_I4.ToInstruction(num));
      body.Instructions.Add(OpCodes.Ret.ToInstruction());
      return body;
    }
   
    private static CilBody return_string(string txt) {
      CilBody body = new CilBody();
      body.Instructions.Add(OpCodes.Ldstr.ToInstruction(txt));
      body.Instructions.Add(OpCodes.Ret.ToInstruction());
      return body;
    }

    private static CilBody return_null() {
      var body = new CilBody();
      body.Instructions.Add(OpCodes.Ldnull.ToInstruction());
      body.Instructions.Add(OpCodes.Ret.ToInstruction());
      return body;
    }
 
    private static CilBody return_empty() {
      var body = new CilBody();
      body.Instructions.Add(OpCodes.Ret.ToInstruction());
      return body;
    }
    
    private static CilBody return_date(int year, int month, int day) {
      var body = new CilBody();

      var systemDateTime = main.CorLibTypes.GetTypeRef("System", "DateTime");

      var ctor = new MemberRefUser(main, ".ctor",
        MethodSig.CreateInstance(
          main.CorLibTypes.Void,
          main.CorLibTypes.Int32,
          main.CorLibTypes.Int32,
          main.CorLibTypes.Int32),
        systemDateTime);


      body.Instructions.Add(OpCodes.Ldc_I4.ToInstruction(year));
      body.Instructions.Add(OpCodes.Ldc_I4.ToInstruction(month));
      body.Instructions.Add(OpCodes.Ldc_I4.ToInstruction(day));
      body.Instructions.Add(OpCodes.Newobj.ToInstruction(ctor));
      body.Instructions.Add(OpCodes.Ret.ToInstruction());
      return body;
    }

    static void Main() {

      Console.SetWindowSize(160, 32);

      try {
        const string installDir = "c:\\Program Files\\Devart\\Code Compare"; //todo: get from registry?
        if (!Directory.Exists(installDir)) {
          Print("Code Compare is not installed");
          Console.ReadKey();
          Environment.Exit(0);
          return;
        }

        PrintColor("[InstallDir]: " + installDir, ConsoleColor.Cyan);

        // var pname = Process.GetProcessesByName("devenv");
        // if (pname.Length > 0) {
        //   PrintColor("[Error]: [devenv.exe is running at the background please kill it!]", ConsoleColor.Red);
        //   Console.ReadKey();
        //   Environment.Exit(0);
        // }

        mainCtx = ModuleDef.CreateModuleContext();
        PrintColor("---- Please Wait -----", ConsoleColor.Cyan);
        PrintColor("[Customer ID]: Anonymous", ConsoleColor.DarkCyan);
        PrintColor("[Customer Key]: secretKey", ConsoleColor.DarkCyan);
        PrintColor("---- Please Wait -----", ConsoleColor.Cyan);

        // Patch Devart.Activation
        var file = installDir + "\\Devart.Activation.dll";
        var fileBackup = file + ".back";
        if (!File.Exists(fileBackup)) File.Copy(file, fileBackup);
        PrintColor("[Backup Path]: " + fileBackup, ConsoleColor.Green);
        PrintColor("[Patching]: " + file, ConsoleColor.Red);
        main = ModuleDefMD.Load(File.ReadAllBytes(file), mainCtx);
        var typeDef = GetType(main, "Devart.Activation.LicenseActivator");
        typeDef.FindMethod("GetLicenseStatusAsync").Body = return_digit(2);
        typeDef.FindMethod("GetLicenseStatus").Body = return_digit(2);
        typeDef.FindMethod("ActivateLicense").Body = return_empty();
        typeDef.FindMethod("CreateActivationRequest").Body = return_null();
        File.SetAttributes(file,  System.IO.FileAttributes.Normal);
        main.NativeWrite(file);

        // Patch Devart.Common
        file = installDir + "\\Devart.Common.dll";
        fileBackup = file + ".back";
        if (!File.Exists(fileBackup)) File.Copy(file, fileBackup);
        PrintColor("[Backup Path]: " + fileBackup, ConsoleColor.Green);
        PrintColor("[Patching]: " + file, ConsoleColor.Red);
        main = ModuleDefMD.Load(File.ReadAllBytes(file), mainCtx);
        typeDef = GetType(main, "Devart.Common.LicenseInfo");
        typeDef.FindMethod("get_Company").Body = return_string("Sergiye.3xtraTools");
        typeDef.FindMethod("get_DaysBeforeExpired").Body = return_digit(999999);
        typeDef.FindMethod("get_EndDate").Body = return_date(2050, 06, 27);
        typeDef.FindMethod("get_IsSubscriptionExpired").Body = return_bool(false);
        typeDef.FindMethod("get_LicenseType").Body = return_digit(2);
        typeDef.FindMethod("get_LicenseNumber").Body = return_string("1234-5678-8765-4321");
        typeDef.FindMethod("get_ProductName").Body = return_string("Code Compare Pro");
        typeDef.FindMethod("get_UserCount").Body = return_digit(10);
        typeDef.FindMethod("get_UserName").Body = return_string("Sergiye");
        typeDef.FindMethod("get_IsExpired").Body = return_bool(false);
        typeDef.FindMethod("Verify").Body = return_bool(true);
        
        // var body = new CilBody {
        //   KeepOldMaxStack = true,
        // };
        // // var getDefault = new MethodDefUser("GetDefault", MethodSig.CreateInstance(typeDef.ToTypeSig())) {
        // // var getDefault = new MethodDefUser("GetDefault", MethodSig.CreateInstance(new ClassSig(typeDef))) {
        // // var getDefault = new MethodDefUser("Default", typeDef.FindDefaultConstructor().MethodSig) {
        //   // Attributes = MethodAttributes.Public,
        //   // ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed
        // // };
        // // typeDef.Methods.Add(getDefault);
        //
        // // var getDefault = new MemberRefUser(main, "Default", MethodSig.CreateInstance(typeDef.ToTypeSig()), typeDef);
        // // var getDefault = new MemberRefUser(main, "Default", MethodSig.CreateInstance(new ClassSig(typeDef)), typeDef);
        // // body.Instructions.Add(OpCodes.Call.ToInstruction(getDefault));
        // // body.Instructions.Add(OpCodes.Call.ToInstruction(typeDef.FindDefaultConstructor()));
        //
        // var writeLine = new MemberRefUser(main, "WriteLine", MethodSig.CreateStatic(main.CorLibTypes.Void, main.CorLibTypes.String), main.CorLibTypes.GetTypeRef("System", "Console"));
        // body.Instructions.Add(OpCodes.Ldstr.ToInstruction("Default .ctor called"));
        // body.Instructions.Add(OpCodes.Call.ToInstruction(writeLine));
        // body.Instructions.Add(OpCodes.Newobj.ToInstruction(MethodSig.CreateInstance(new ClassSig(typeDef))));
        // body.Instructions.Add(OpCodes.Ret.ToInstruction());
        // typeDef.FindMethod("Parse").Body = body;
        File.SetAttributes(file,  System.IO.FileAttributes.Normal);
        main.NativeWrite(file);

        // Patch Devart.CodeCompare
        file = installDir + "\\Devart.CodeCompare.dll";
        fileBackup = file + ".back";
        if (!File.Exists(fileBackup)) File.Copy(file, fileBackup);
        PrintColor("[Backup Path]: " + fileBackup, ConsoleColor.Green);
        PrintColor("[Patching]: " + file, ConsoleColor.Red);
        main = ModuleDefMD.Load(File.ReadAllBytes(file), mainCtx);
        typeDef = GetType(main, "Devart.CodeCompare.LicenseChecker");
        typeDef.FindMethod("CheckLicense").Body = return_bool(true);
        typeDef.FindMethod("CheckLicenseUpdate").Body = return_bool(false);

        typeDef = GetType(main, "Devart.CodeCompare.CodeCompareProductInfo");
        typeDef.FindMethod("get_IsTrial").Body = return_bool(false);
        typeDef.FindMethod("get_LicenseSpecified").Body = return_bool(true);
        
        typeDef = GetType(main, "Devart.CodeCompare.ProductInfo");
        typeDef.FindMethod("get_LaysenseStatus").Body = return_digit(2);
        typeDef.FindMethod("ValidateLaysense").Body = return_empty();
        
        File.SetAttributes(file,  System.IO.FileAttributes.Normal);
        main.NativeWrite(file);

        PrintColor("[Finish]: Press any key to continue...", ConsoleColor.Green);
      }
      catch (Exception ex) {
        PrintColor("[Error]: " + ex.Message, ConsoleColor.Red);
      }

      Console.ReadKey();
      Environment.Exit(0);
    }
  }
}