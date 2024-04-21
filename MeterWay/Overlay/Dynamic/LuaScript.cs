using System;
using System.IO;
using NLua;
using NLua.Exceptions;

namespace Dynamic;

public class LuaScript : IDisposable
{
    public FileInfo FilePath { get; private init; }
    public string Name { get; private init; }
    public Function[] Functions { get; private init; }

    private Lua? LuaInterpreter { get; set; }
    private LuaFunction? Script { get; set; }
    private LuaFunction? Draw { get; set; }
    private LuaFunction? DrawConfig { get; set; }

    public bool HasDraw => Draw != null;
    public bool HasDrawConfig => DrawConfig != null;
    public bool Status => LuaInterpreter != null;

    public LuaScript(FileInfo filePath, Function[] functions)
    {
        FilePath = filePath;
        Name = Path.GetFileNameWithoutExtension(FilePath.FullName);
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
    public void ExecuteDrawTab() => Executefunction(DrawConfig);
    public void Execute() => Executefunction(DrawConfig);

    public void Dispose()
    {
        LuaInterpreter?.Dispose();

        Script?.Dispose();
        Draw?.Dispose();
        DrawConfig?.Dispose();
    }

    private void Init()
    {
        LuaInterpreter = new();
        LuaInterpreter.LoadCLRPackage();
        RegisterFunctions();

        Script = LoadScript(FilePath);

        Draw = GetFunction("Draw");
        DrawConfig = GetFunction("DrawConfig");
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
            var function = LuaInterpreter?.GetFunction(functionName);
            if (function == default) throw new Exception("Function is empty");
            return function;
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
            LuaInterpreter = null;
        }
    }
}
