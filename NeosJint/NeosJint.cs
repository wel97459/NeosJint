//Code by @0utsider89#8249

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseX;
using FrooxEngine;
using Jint;
using System.Security.Cryptography;

namespace NeosJint
{

    [Category("LogiX/Script")]
    [NodeName("JavaScript")]

    public class NeosJint : LogixNode
    {
        public readonly SyncList<Sync<string>> Script;

        public readonly Sync<string> Content;
        public readonly Sync<string> ContentOut;
        public readonly Sync<string> DebugError;
        public readonly Sync<bool> RefreshPorts;
        public readonly Sync<bool> Refresh;
        public readonly Sync<bool> impulseRun;

        public readonly Impulse NewData;
        public readonly SyncList<SyncVar> DataOut;
        public readonly SyncList<SyncVar> Outputs;
        public readonly SyncList<SyncVar> Inputs;
        public readonly SyncList<SyncVar> Syncs;

        bool RefreshPortsOld;
        bool RefreshOld;

        Jint.Engine engine;

        [ImpulseTarget]
        public void Run()
        {
            impulseRun.Value = true;
        }

        protected override void OnEvaluate()
        {
            for (int i = 0; i < Outputs.Count; i++)
            {
                if (Outputs.GetElement(i).Element is Output<bool>)
                    (Outputs.GetElement(i).Element as Output<bool>).Value = (DataOut.GetElement(i).Element as Sync<bool>).Value;

                if (Outputs.GetElement(i).Element is Output<string>)
                    (Outputs.GetElement(i).Element as Output<string>).Value = (DataOut.GetElement(i).Element as Sync<string>).Value;

                if (Outputs.GetElement(i).Element is Output<int>)
                    (Outputs.GetElement(i).Element as Output<int>).Value = (DataOut.GetElement(i).Element as Sync<int>).Value;

                if (Outputs.GetElement(i).Element is Output<float>)
                    (Outputs.GetElement(i).Element as Output<float>).Value = (DataOut.GetElement(i).Element as Sync<float>).Value;

                if (Outputs.GetElement(i).Element is Output<float3>)
                    (Outputs.GetElement(i).Element as Output<float3>).Value = (DataOut.GetElement(i).Element as Sync<float3>).Value;

                if (Outputs.GetElement(i).Element is Output<color>)
                    (Outputs.GetElement(i).Element as Output<color>).Value = (DataOut.GetElement(i).Element as Sync<color>).Value;

                if (Outputs.GetElement(i).Element is Output<floatQ>)
                    (Outputs.GetElement(i).Element as Output<floatQ>).Value = (DataOut.GetElement(i).Element as Sync<floatQ>).Value;
            }
        }

        protected override void OnAttach()
        {
            Script.Add().Value = "var cube = World.RootSlot.FindInChildren(\"Cube\");";
            Script.Add().Value = "var cube1 = World.RootSlot.FindInChildren(\"Cube1\");";
            Script.Add().Value = "function InitPort(){AddOut(\"floatQ\");}";
            Script.Add().Value = "";
            Script.Add().Value = "function Update(){";
            Script.Add().Value = "if(cube1.Parent == World.RootSlot){";
            Script.Add().Value = "OutFloatQ(0, cube.GlobalRotation);";
            Script.Add().Value = "cube1.Rotation_Field.Value = cube.GlobalRotation;";
            Script.Add().Value = "}"; 
            Script.Add().Value = "}";
            //Script.Add().Value = "function Run(){t++;}";
            //World.RootSlot.FindInChildren("Cube").GlobalPosition
            RefreshOld = false;
            RefreshPortsOld = false;
            loadJS();
            InitPorts();
        }

        protected override void OnCommonUpdate()
        {
            base.OnCommonUpdate();

            try
            {
                engine.Execute("Update();");
            }
            catch (Jint.Parser.ParserException E)
            {
                DebugError.Value = "Update() Line " + E.LineNumber + " : " + E.Description;
            }
            catch (Exception) { }
            if (impulseRun.Value)
            {
                try
                {
                    engine.Execute("Run();");
                }
                catch (Jint.Parser.ParserException E)
                {
                    DebugError.Value = "Run() Line " + E.LineNumber + " : " + E.Description;
                }
                catch (Exception)
                {
                }
                impulseRun.Value = false;
            }

        }

        protected override void OnChanges()
        {
            base.OnChanges();

            if (RefreshOld != Refresh.Value)
            {
                loadJS();
                RefreshOld = Refresh.Value;
            }

            if (RefreshPortsOld != RefreshPorts.Value)
            {
                InitPorts();
                RefreshPortsOld = RefreshPorts.Value;
            }

            if (!string.IsNullOrEmpty(Content.Value))
            {
                Script.Clear();
                string[] lines = Content.Value.Split('\n');
                foreach (var line in lines)
                {
                    Script.Add().Value = line;
                }
                Content.Value = "";
            }
        }

        private void loadJS()
        {
            var js = Script.GetElement(0).Value;
            for (int i = 1; i < Script.Count; i++)
                js += "\n" + Script.GetElement(i).Value;

            ContentOut.Value = js;

            engine = new Jint.Engine();

            //Output Types
            engine.SetValue("OutputBool", new Action<int, bool>(OutputValue<bool>));
            engine.SetValue("OutputString", new Action<int, string>(OutputValue<string>));
            engine.SetValue("OutputInt", new Action<int, int>(OutputValue<int>));
            engine.SetValue("OutputFloat", new Action<int, float>(OutputValue<float>));
            engine.SetValue("OutputFloat3", new Action<int, float3>(OutputValue<float3>));
            engine.SetValue("OutputColor", new Action<int, color>(OutputValue<color>));
            engine.SetValue("OutputFloatQ", new Action<int, floatQ>(OutputValue<floatQ>));

            //Input Types
            engine.SetValue("InputBool", new Func<int, bool>(InputValue<bool>));
            engine.SetValue("InputString", new Func<int, string>(InputValue<string>));
            engine.SetValue("InputInt", new Func<int, int>(InputValue<int>));
            engine.SetValue("InputFloat", new Func<int, float>(InputValue<float>));
            engine.SetValue("InputFloat3", new Func<int, float3>(InputValue<float3>));
            engine.SetValue("InputColor", new Func<int, color>(InputValue<color>));
            engine.SetValue("InputFloatQ", new Func<int, floatQ>(InputValue<floatQ>));


            //Output Types
            engine.SetValue("SetBool", new Action<int, bool>(PutValue<bool>));
            engine.SetValue("SetString", new Action<int, string>(PutValue<string>));
            engine.SetValue("SetInt", new Action<int, int>(PutValue<int>));
            engine.SetValue("SetFloat", new Action<int, float>(PutValue<float>));
            engine.SetValue("SetFloat3", new Action<int, float3>(PutValue<float3>));
            engine.SetValue("SetColor", new Action<int, color>(PutValue<color>));
            engine.SetValue("SetFloatQ", new Action<int, floatQ>(PutValue<floatQ>));

            //Input Types
            engine.SetValue("GetBool", new Func<int, bool>(GetValue<bool>));
            engine.SetValue("GetString", new Func<int, string>(GetValue<string>));
            engine.SetValue("GetInt", new Func<int, int>(GetValue<int>));
            engine.SetValue("GetFloat", new Func<int, float>(GetValue<float>));
            engine.SetValue("GetFloat3", new Func<int, float3>(GetValue<float3>));
            engine.SetValue("GetColor", new Func<int, color>(GetValue<color>));
            engine.SetValue("GetFloatQ", new Func<int, floatQ>(GetValue<floatQ>));

            //Function
            engine.SetValue("Log", new Action<object, bool>(Debug.Log));
            engine.SetValue("AddIn", new Action<string>(AddInput));
            engine.SetValue("AddOut", new Action<string>(AddOutput));
            engine.SetValue("AddSync", new Action<string>(AddSync));

            //Objects
            engine.SetValue("Time", base.Time);
            engine.SetValue("MySlot", base.Slot);
            engine.SetValue("World", base.World);
            
            //Types
            engine.SetValue("string", typeof(string));
            engine.SetValue("int", typeof(int));
            engine.SetValue("float", typeof(float));
            engine.SetValue("float3", typeof(float3));
            engine.SetValue("color", typeof(color));
            engine.SetValue("slot", typeof(Slot));
            engine.SetValue("floatQ", typeof(floatQ));

            try
            {
                engine.Execute(js);
            }
            catch (Jint.Parser.ParserException E)
            {
                DebugError.Value = "Line " + E.LineNumber + " : " + E.Description;
                Debug.Log("Line " + E.LineNumber + " : " + E.Description);
            }
            catch (Exception E)
            {
                Debug.Log("NeosJint Error: " + E.Message);
            }
        }

        public void OutputValue<T>(int output = 0, T val = default(T))
        {
            (DataOut.GetElement(output).Element as Sync<T>).Value = val;
        }

        public T InputValue<T>(int inputNumber = 0)
        {
            var input = Inputs.GetElement(inputNumber).Element as Input<T>;
            if (input == null)
                return default(T);
            return input.EvaluateRaw();
        }

        public T GetValue<T>(int inputNumber = 0)
        {
            var input = Syncs.GetElement(inputNumber).Element as Sync<T>;
            if (input == null)
                return default(T);
            return input.Value;
        }

        public void PutValue<T>(int output = 0, T val = default(T))
        {
            (Syncs.GetElement(output).Element as Sync<T>).Value = val;
        }

        public void AddInput(string type = "bool")
        {
            if (type == "bool")
                Inputs.Add().ElementType = typeof(Input<string>);
            if (type == "string")
                Inputs.Add().ElementType = typeof(Input<string>);
            if (type == "int")
                Inputs.Add().ElementType = typeof(Input<int>);
            if (type == "float")
                Inputs.Add().ElementType = typeof(Input<float>);
            if (type == "float3")
                Inputs.Add().ElementType = typeof(Input<float3>);
            if (type == "color")
                Inputs.Add().ElementType = typeof(Input<color>);
            if (type == "floatQ")
                Inputs.Add().ElementType = typeof(Input<floatQ>);
        }
        public void AddOutput(string type = "bool")
        {
            if (type == "bool")
            {
                Outputs.Add().ElementType = typeof(Output<bool>);
                DataOut.Add().ElementType = typeof(Sync<bool>);
            }
            if (type == "string")
            {
                Outputs.Add().ElementType = typeof(Output<string>);
                DataOut.Add().ElementType = typeof(Sync<string>);
            }
            if (type == "int")
            {
                Outputs.Add().ElementType = typeof(Output<int>);
                DataOut.Add().ElementType = typeof(Sync<int>);
            }
            if (type == "float")
            {
                Outputs.Add().ElementType = typeof(Output<float>);
                DataOut.Add().ElementType = typeof(Sync<float>);
            }
            if (type == "float3")
            {
                Outputs.Add().ElementType = typeof(Output<float3>);
                DataOut.Add().ElementType = typeof(Sync<float3>);
            }
            if (type == "color")
            {
                Outputs.Add().ElementType = typeof(Output<color>);
                DataOut.Add().ElementType = typeof(Sync<color>);
            }
            if (type == "floatQ")
            {
                Outputs.Add().ElementType = typeof(Output<floatQ>);
                DataOut.Add().ElementType = typeof(Sync<floatQ>);
            }
        }

        public void AddSync(string type = "bool")
        {
            if (type == "bool")
                Syncs.Add().ElementType = typeof(Sync<bool>);
            if (type == "string")
                Syncs.Add().ElementType = typeof(Sync<string>);
            if (type == "int")
                Syncs.Add().ElementType = typeof(Sync<int>);
            if (type == "float")
                Syncs.Add().ElementType = typeof(Sync<float>);
            if (type == "float3")
                Syncs.Add().ElementType = typeof(Sync<float3>);
            if (type == "color")
                Syncs.Add().ElementType = typeof(Sync<color>);
            if (type == "floatQ")
                Syncs.Add().ElementType = typeof(Sync<floatQ>);
        }

        public void InitPorts()
        {
            Inputs.Clear();
            Outputs.Clear();
            DataOut.Clear();
            Syncs.Clear();

            try
            {

                engine.Execute("InitPort();");
            }
            catch (Jint.Parser.ParserException E)
            {
                DebugError.Value = "InitPort() Line " + E.LineNumber + " : " + E.Description;
            }
            catch (Exception) { }

            RefreshLogixBox();
        }

        private void test()
        {
            //Slot s;
            //s = World.RootSlot;
            //base.World.RootSlot.FindInChildren
            //s.Rotation_Field;   
        }
    }
}
