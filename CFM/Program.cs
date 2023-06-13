using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using RestSharp;

namespace CFM;

internal static class Program
{
    private static string workDir = AppDomain.CurrentDomain.BaseDirectory + "work/";
    private static string confDir = AppDomain.CurrentDomain.BaseDirectory + "conf/";
    private static List<string> bannedIp = new();
    private static Dictionary<string,string> conf = new();
#if RELEASE
        private const string Version = "1.0.0";
#else
    private const string Version = "develop";
    private static bool _debug;
#endif

    private static void Main(string[] args)
    {
        if (!Directory.Exists(workDir)) Directory.CreateDirectory(workDir);
        if (!Directory.Exists(confDir)) Directory.CreateDirectory(confDir);
        LoadConfig();

        bannedIp.AddRange(GetBandedIps());

        if (args.Length >= 1)
        {
            switch (args[0])
            {
                case "--version":
                case "--ver":
                case "--v":
                    Console.WriteLine("CFM Ver: " + Version);
                    break;
                case "added":
                    if (bannedIp.Contains(args[1]))
                    {
                        Console.WriteLine($"Fail Added IP {args[1]}");
                        return;
                    }

                    bannedIp.Add(args[1]);
                    Console.WriteLine($"Added IP {args[1]}");
                    SaveBandedIps();
                    CfUpdate();
                    break;
                case "delete":
                    if (!bannedIp.Contains(args[1]))
                    {
                        Console.WriteLine($"Fail Delete IP {args[1]}");
                        return;
                    }

                    bannedIp.Remove(args[1]);
                    Console.WriteLine($"Delete IP {args[1]}");
                    SaveBandedIps();
                    CfUpdate();
                    break;
                case "setup-terminal":
                    SetupTerminal();
                    break;
                // case "setup":
                //     break;
                case "help":
                    EchoHelpMessage();
                    break;
                case "generated":
                    CfUpdate();
                    break;
                default:
                    Console.WriteLine($"The parameter \"{args[0]}\" is incorrect.");
                    break;
            }
        }
        else
        {
            Console.WriteLine("If you want to see help messages, please add --help");
        }
    }
    
    // Settings

    private static void SetupTerminal()
    {
        cft:
        Console.Write("CloudFlare Token > ");
        String token = Console.ReadLine()!;
        if (token == "")
        {
            goto cft;
        }
        
        zone:
        Console.Write("CloudFlare ZoneID > ");
        String zone = Console.ReadLine()!;
        if (zone == "")
        {
            goto zone;
        }
        
        
        rulesetid:
        Console.Write("CloudFlare RuleSetID > ");
        String ruleset = Console.ReadLine()!;
        if (zone == "")
        {
            goto rulesetid;
        }

        ruleid:
        Console.Write("CloudFlare RuleID > ");
        String rule = Console.ReadLine()!;
        if (zone == "")
        {
            goto ruleid;
        }
        
        conf.Clear();
        conf.Add("cf-token",token);
        conf.Add("cf-zone-id",zone);
        conf.Add("cf-ruleset-id",ruleset);
        conf.Add("cf-rule-id",rule);
        SaveConfig();
    }

    private static bool LoadConfig()
    {
        conf.Clear();
        if (isCreatedConf())
        {
            foreach (string o in FileManager.ReadFile(confDir+"settings.ini"))
            {
                var a = o.Split(" : ");
                conf.Add(a[0],a[1]);
            }

            return true;
        }
        SetupTerminal();
        // Console.WriteLine("Configuration does not exist Run \"CFM --setup-terminal\"");
        // Environment.Exit(-1);
        return false;
    }

    private static void SaveConfig()
    {
        List<string> tmp = new List<string>();
        foreach (var keyValuePair in conf.Keys)
        {
            tmp.Add(keyValuePair+" : "+conf[keyValuePair]);
        }
        FileManager.WriteFile(confDir+"settings.ini",tmp);
    }

    private static bool isCreatedConf()
    {
        return File.Exists(confDir + "settings.ini");
    }
    
    // Message

    private static void EchoHelpMessage()
    {
        Console.WriteLine("Coming Soon.");
    }
    
    // BanndedIp

    private static string[] GetBandedIps()
    {
        if (File.Exists(workDir + "ips.cfm"))
        {
            return (string[])FileManager.ReadFile(workDir + "ips.cfm").ToArray(typeof(string));
        }

        return new string[] { };
    }

    private static bool SaveBandedIps()
    {
        if (File.Exists(workDir + "ips.cfm"))
        {
            FileManager.WriteFile(workDir + "ips.cfm", bannedIp);
            return true;
        }
        Console.WriteLine("Failed to save BannedIps");
        return false;
    }
    
    private static void CfUpdate()
    {
        if (bannedIp.Count is 0)
        {
            bannedIp.Add("1.1.1.1");
        }
        string ips = "";
        List<string> tmp = new List<string>();
        foreach (var s in bannedIp)
        {
            tmp.Add("ip.src eq "+s);
        }

        ips = string.Join(") or (",tmp);
        
        var client =
            new RestClient(
                $"https://api.cloudflare.com/client/v4/zones/{conf["cf-zone-id"]}/rulesets/{conf["cf-ruleset-id"]}/rules/{conf["cf-rule-id"]}");
        var request = new RestRequest();
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", $"Bearer {conf["cf-token"]}");
        request.AddParameter("application/json", "{" +
                                                 "  \"action\": \"block\"," +
                                                 "  \"expression\": \"("+ips+")\"," +
                                                 "  \"description\": \"CFM Auto Generated\"" +
                                                 "}",
            ParameterType.RequestBody);
        
        Console.WriteLine(client.Patch(request).Content);
        
        Console.WriteLine("Blocked IPs: " + string.Join(" , ",bannedIp));
    }
    
    
}