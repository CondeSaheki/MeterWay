using System;
using System.IO;
using NLua;
using NLua.Exceptions;

namespace Dynamic;

public class LuaScript : IDisposable
{
    public FileInfo FilePath { get; init; }
    public string Name { get; init; }
    public Function[] Functions { get; init; }

    private Lua? LuaInterpreter { get; set; }
    private LuaFunction? Script { get; set; }
    private LuaFunction? Draw { get; set; }
    private LuaFunction? DrawTab { get; set; }

    public LuaScript(string name, string filePath, Function[] functions)
    {
        FilePath = new FileInfo(filePath);
        Name = name;
        Functions = functions;
        Init();
    }

    public void Reload()
    {
        Dispose();
        Init();
    }

    public class Function(string name, object? target, System.Reflection.MethodInfo? method)
    {
        public string Name = name;
        public object? Target = target;
        public System.Reflection.MethodInfo? Method = method;
    }

    public void ExecuteDraw() => Executefunction(Draw);
    public void ExecuteDrawTab() => Executefunction(DrawTab);
    public void Execute() => Executefunction(DrawTab);

    public void Dispose()
    {
        LuaInterpreter?.Dispose();

        Script?.Dispose();
        Draw?.Dispose();
        DrawTab?.Dispose();
    }

    private void Init()
    {
        LuaInterpreter = new();
        LuaInterpreter.LoadCLRPackage();
        RegisterFunctions();

        Script = LoadScript(FilePath);

        Draw = GetFunction("Draw");
        DrawTab = GetFunction("DrawTab");
    }

    private void RegisterFunctions()
    {
        foreach (var function in Functions)
        {
            try
            {
                LuaInterpreter?.RegisterFunction(function.Name, function.Target, function.Method);
            }
            catch (Exception ex)
            {
                MeterWay.Dalamud.Log.Warning($"Error registering function \'{function.Name}\':\n{ex}");
                LuaInterpreter = null;
                break;
            }
        }
    }

    private LuaFunction? LoadScript(FileInfo filePath)
    {
        if (LuaInterpreter == null) return null;
        if (filePath == null || !filePath.Exists)
        {
            MeterWay.Dalamud.Log.Warning($"Script \'{filePath}\' do not exists");
            return null;
        }
        string script = string.Empty;

        MeterWay.Dalamud.Log.Info($"Reading: {filePath.FullName}");
        try
        {
            script = File.ReadAllText(filePath.FullName);
        }
        catch (Exception ex)
        {
            MeterWay.Dalamud.Log.Warning($"Error reading file \'{filePath.FullName}\':\n{ex}");
            return null;
        }
        try
        {
            return LuaInterpreter.LoadString(script, Name);
        }
        catch (Exception ex)
        {
            MeterWay.Dalamud.Log.Warning($"Error loading script from \'{filePath.FullName}\':\n{ex}");
        }
        return null;
    }

    private LuaFunction? GetFunction(string functionName)
    {
        if (LuaInterpreter == null) return null;
        try
        {
            return LuaInterpreter?.GetFunction(functionName);
        }
        catch (LuaException ex)
        {
            MeterWay.Dalamud.Log.Error($"Failed to get Lua function \'{functionName}\':\n{ex}");
        }
        return null;
    }

    private void Executefunction(LuaFunction? function)
    {
        if (function == null || LuaInterpreter == null) return;
        try
        {
            function.Call();
        }
        catch (Exception ex)
        {
            MeterWay.Dalamud.Log.Error($"Error running Lua script:\n{ex}");
        }
    }
}
