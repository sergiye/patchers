using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Microsoft.Win32;
using FileAttributes = System.IO.FileAttributes;

namespace ResharperPatcher {

  internal class Program {

    private static void PrintColor(string message, ConsoleColor color) {
      var pieces = Regex.Split(message, @"(\[[^\]]*\])");
      foreach (var t in pieces) {
        var piece = t;
        if (piece.StartsWith("[") && piece.EndsWith("]")) {
          Console.ForegroundColor = color;
          piece = piece.Substring(1, piece.Length - 2);
        }
        Console.Write(piece);
        Console.ResetColor();
      }
      Console.WriteLine();
    }

    private static TypeDef GetType(ModuleDefMD module, string classPath) {
      return module.Types.FirstOrDefault(type => type.FullName == classPath);
    }

    /*
            private static void debug_body(CilBody body)
            {
                for (int i = 0; i < body.Instructions.Count; i++)
                {
                    var instr = body.Instructions[i];
                    Print(instr.ToString());
                }
            }
    */

    #region Cil methods

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

    private static CilBody return_date(ModuleDefMD main, int year, int month, int day) {
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

    private static CilBody return_id(string userKey) {
      var keyPart = userKey.IndexOf('-');
      int.TryParse(userKey.Substring(0, keyPart), out var num);
      return return_digit(num);
    }

    #endregion

    private static void PatchJetBrainsLicense(ModuleDefMD main , string user, string key) {
      PrintColor("---- Please Wait -----", ConsoleColor.Cyan);
      PrintColor("[Customer Key]: " + key, ConsoleColor.DarkCyan);
      PrintColor("[Customer ID]: " + user, ConsoleColor.DarkCyan);
      PrintColor("---- Please Wait -----", ConsoleColor.Cyan);
      // Class LicenseChecker
      var licenseChecker = GetType(main, "JetBrains.Application.License.LicenseChecker");
      // Class LicenseData
      var licenseData = GetType(main, "JetBrains.Application.License.LicenseData");
      // Class LicenseData
      var resultEx = GetType(main, "JetBrains.Application.License2.ResultEx");
      // Class UserLicenseViewSubmodel
      var userLicenseViewSubmodel = GetType(main, "JetBrains.Application.License2.UserLicenses.UserLicenseViewSubmodel");
      // Class UserLicenseStatus
      var userLicenseStatus = GetType(main, "JetBrains.Application.License2.UserLicenses.UserLicenseStatus");
      // Class ResultWithDescription
      var resultWithDescription = GetType(main, "JetBrains.Application.License2.ResultWithDescription");
      // Class LicensedEntityEx
      var licensedEntityEx = GetType(main, "JetBrains.Application.License2.LicensedEntityEx");
      // Class LicensedEntityEx
      //var evaluationLicenseViewSubmodel = GetType(_main, "JetBrains.Application.License2.Evaluation.EvaluationLicenseViewSubmodel");
      //////////////////////////////////////////////////////////////////////
      // Patch get_IsChecksumOK
      licenseChecker.FindMethod("get_IsChecksumOK").Body = return_bool(true);
      // Patch HasLicense
      licenseChecker.FindMethod("get_HasLicense").Body = return_bool(true);
      // Patch HasLicense
      licenseChecker.FindMethod("get_Type").Body = return_digit(0);
      // Patch CustomerId
      licenseChecker.FindMethod("get_CustomerId").Body = return_id(key);
      // Patch get_ExpirationDate
      licenseData.FindMethod("get_ExpirationDate").Body = return_date(main, 2030, 5, 5);
      // Patch get_SubscriptionEndDate
      licenseData.FindMethod("get_SubscriptionEndDate").Body = return_date(main, 2030, 5, 5);
      // Patch get_GenerationDate
      licenseData.FindMethod("get_GenerationDate").Body = return_date(main, 2020, 5, 5);
      // Patch get_IsEndless
      licenseData.FindMethod("get_IsEndless").Body = return_bool(true);
      // Patch get_ContainsSubscription
      licenseData.FindMethod("get_ContainsSubscription").Body = return_bool(true);
      // Patch get_LicenseKey
      licenseData.FindMethod("get_LicenseKey").Body = return_string(key);
      // Patch get_UserName
      licenseData.FindMethod("get_UserName").Body = return_string(user);
      // Patch customer id
      licenseData.FindMethod("get_CustomerId").Body = return_id(key);
      // Patch get_LicenseType
      licenseData.FindMethod("get_LicenseType").Body = return_digit(0);
      // Patch result
      resultWithDescription.FindMethod("get_Result").Body = return_digit(0);
      // RequiresLicense [ EA8 PATCH ]
      licensedEntityEx.FindMethod("RequiresLicense").Body = return_bool(false);
      // Patch TryCreateInfoForOldLicenseData 
      userLicenseViewSubmodel.FindMethod("TryCreateInfoForOldLicenseData").Body = return_null();
      // Patch CheckLicense [ EA8 PATCH ]
      userLicenseViewSubmodel.FindMethod("CheckLicense").Body = return_empty();
      resultEx.FindMethod("IsSuccessful").Body = return_bool(true);
      resultEx.FindMethod("ContainsWarnings").Body = return_bool(false);
      resultEx.FindMethod("Is30MinToShutdown").Body = return_bool(false);
      resultEx.FindMethod("IsFailed").Body = return_bool(false);
      // Patch get_Severity
      userLicenseStatus.FindMethod("get_Severity").Body = return_digit(0);
    }

    private static IEnumerable<string> GetInstallDirs() {
      
      var vsVersions = new [] {
        "SOFTWARE\\JetBrains\\ReSharperPlatformVs17\\", 
        "SOFTWARE\\JetBrains\\ReSharperPlatformVs16\\",
        "SOFTWARE\\JetBrains\\dotCover\\",
        "SOFTWARE\\JetBrains\\dotMemory\\",
        // "SOFTWARE\\JetBrains\\dotPeek\\", //already free
        "SOFTWARE\\JetBrains\\dotTrace\\",
      };

      foreach (var vsVersion in vsVersions) {
        var jbKey = Registry.CurrentUser.OpenSubKey(vsVersion);
        var reSharperPlatform = jbKey?.GetSubKeyNames();
        if (reSharperPlatform == null) continue;
        foreach (var sk in reSharperPlatform) {
          using (var key = jbKey.OpenSubKey(sk)) {
            if (key?.GetValue("InstallDir") is not string installDir) continue;
            PrintColor("[ReSharper]: " + sk, ConsoleColor.Cyan);
            yield return installDir;
          }
        }
      }

      yield return Environment.CurrentDirectory; //for portable apps like dotMemory.UI
    }

    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedParameter.Local
    static void Main(string[] args) {
      if (args is null) { }

      //Console.SetWindowSize(160, 32);

      try {
        var pName = Process.GetProcessesByName("devenv");
        if (pName.Length > 0) {
          PrintColor("[Error]: [devenv.exe is running at the background please kill it!]", ConsoleColor.Red);
          return;
        }

        var found = false;
        foreach(var installDir in GetInstallDirs()) {

          var file = installDir + "\\JetBrains.Platform.Shell.dll";
          if (!File.Exists(file))
            continue;

          found = true;
          PrintColor("[InstallDir]: " + installDir, ConsoleColor.Cyan);
          
          var fileBackup = installDir + "\\JetBrains.Platform.Shell.dll.back";
          if (!File.Exists(fileBackup)) {
            File.Copy(file, fileBackup);
            PrintColor("[Backup Path]: " + fileBackup, ConsoleColor.Green);
          }

          PrintColor("[Patching]: " + file, ConsoleColor.Red);
          var mainCtx = ModuleDef.CreateModuleContext();
          // dont feed file directly here feed it with bytes so you can replace the file
          var main = ModuleDefMD.Load(File.ReadAllBytes(file), mainCtx);
          PatchJetBrainsLicense(main, "Sergiye.3xtraTools", "1234-5678-8765-4321");
          File.SetAttributes(file, FileAttributes.Normal);
          main.NativeWrite(file);
          //File.Copy(file_temp, file+".fuck", true);
        }

        if (!found) {
          PrintColor("[Error]: ReSharper installation is not found", ConsoleColor.Red);
        }
      }
      catch (Exception ex) {
        PrintColor("[Error]: " + ex.Message, ConsoleColor.Red);
      }
      finally {
        PrintColor("[Finish]: Press any key to continue...", ConsoleColor.Green);
        Console.ReadKey();
        Environment.Exit(0);
      }
    }
  }
}