// See https://aka.ms/new-console-template for more information

using GameDataTool;


void processArgs(string[] args, Dictionary<string, string> dArgs)
{
    for(int index = 0; index < args.Length; index++)
    {
        switch(args[index])
        {
            case "-packFromFolder":
                dArgs.Add("packFromFolder", args[++index]);
                break;
            case "-outFolder":
                dArgs.Add("outFolder", args[++index]);
                break;
            case "-outBaseName":
                dArgs.Add("outBaseName", args[++index]);
                break;
            case "-useChecksum":
                dArgs.Add("useChecksum", "true");
                break;
            case "-testingCycle":
                dArgs.Add("testingCycle", "true");
                break;
            default:

                break;
        }
    }

    return;
}


// actual program starts here:
// sample args line: -packFromFolder "e:\Users\justin\Documents\iRacing" -outBaseName "thisIsATest" -useChecksum -outFolder e:\Users\justin\Documents\iRacing\test10 -testingCycle

bool bSuccess = true;

Console.WriteLine("*******************************************");
Console.WriteLine("GameDataTool - Data Packager - BEGIN");
Console.WriteLine("-------------------------------------------");


// fill a dictionary with the args in a more useful accessing format
Dictionary<string, string> dictArgs = new Dictionary<string, string>();
processArgs(args, dictArgs);

DataPackage dpPackage = new DataPackage();

if(dictArgs.ContainsKey("packFromFolder"))
{
    if( dpPackage.addFromFolder(dictArgs["packFromFolder"]) )
    {
        Console.WriteLine("\t- Added all supported asset files from this folder: " + dictArgs["packFromFolder"]);
    }
    else
    {
        bSuccess = false;
        Console.WriteLine("\t- Failed to add assets from this folder: " + dictArgs["packFromFolder"]);
    }
}

if(bSuccess && (dictArgs.ContainsKey("outFolder")) && (dictArgs.ContainsKey("outBaseName")))
{
    if( dpPackage.generatePKDFile(dictArgs["outFolder"], dictArgs["outBaseName"], true) )
    {
        Console.WriteLine("\t- Generated the new PKD file in this folder: " + dictArgs["outFolder"]);
    }
    else
    {
        bSuccess = false;
        Console.WriteLine("\t- Failed to generate a PKD file in this folder: " + dictArgs["outFolder"]);
    }
}

// now, if user wants to test, extract the files from within the just generated PKD (to confirm a comparison match)
if(bSuccess && (dictArgs.ContainsKey("testingCycle")) && (dictArgs.ContainsKey("outFolder")))
{
    dpPackage.clear();

    foreach (string strFilePath in Directory.GetFiles(dictArgs["outFolder"]))
    {
        if(Path.GetExtension(strFilePath).ToUpper() == ".PKD")
        {
            if( dpPackage.loadFromPKDFile(strFilePath) )
            {
                Console.WriteLine("\t- TESTING - Added all asset files from this PKD: " + strFilePath);
            }
            else
            {
                bSuccess = false;
                Console.WriteLine("\t- TESTING - Failed to add assets from this PKD: " + strFilePath);
                break;
            }
        }
    }
    
    if(bSuccess && dpPackage.extractPKDFilesToPath(dictArgs["outFolder"]) )
    {
        Console.WriteLine("\t- TESTING - Extracted all asset files to this folder: " + dictArgs["outFolder"]);
    }
    else
    {
        bSuccess = false;
        Console.WriteLine("\t- TESTING - Failed to extract the asset files to this folder: " + dictArgs["outFolder"]);
    }
}

Console.WriteLine("-------------------------------------------");
Console.WriteLine("GameDataTool - Data Packager - END");
Console.WriteLine("*******************************************");

Environment.Exit(bSuccess ? 0 : 1);
