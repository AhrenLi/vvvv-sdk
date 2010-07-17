#region licence/info

//////project name
//vvvv plugin template

//////description
//basic vvvv node plugin template.
//Copy this an rename it, to write your own plugin node.

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop 

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.Utils.VColor;
//VVVV.Utils.VMath;

//////initial author
//vvvv group

#endregion licence/info

//use what you need
using System;
//using System.Drawing;
using VVVV.PluginInterfaces.V1;
//using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

//the vvvv node namespace
namespace VVVV.Nodes
{

    //class definition
    public class PluginTemplate : IPlugin, IDisposable
    {
        #region field declaration

        //the host (mandatory)
        private IPluginHost FHost;
        // Track whether Dispose has been called.
        private bool FDisposed = false;

        private double rho = 28;
        private double sigma = 10;
        private double beta = 2.667;
        private double deltaT = 0.01;

        //input pin declaration

        // position vector coordinates.
        private IValueIn FValueInputX;
        private IValueIn FValueInputY;
        private IValueIn FValueInputZ;

        //a kind of fake input needed for feedback.
        private double[] previousValueSliceX;
        private double[] previousValueSliceY;
        private double[] previousValueSliceZ;

        //time.
        private IValueIn FValueInputDeltaT;

        private IValueIn FValueInputRho;
        private IValueIn FValueInputBeta;
        private IValueIn FValueInputSigma;

        //output pin declaration
        private IValueOut FValueOutputX;
        private IValueOut FValueOutputY;
        private IValueOut FValueOutputZ;

        #endregion field declaration

        #region constructor/destructor

        public PluginTemplate()
        {
            //the nodes constructor
            //nothing to declare for this node
        }

        // Implementing IDisposable's Dispose method.
        // Do not make this method virtual. 	
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!FDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                }
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.

                FHost.Log(TLogType.Debug, "PluginTemplate is being deleted");

                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }
            FDisposed = true;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~PluginTemplate()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }
        #endregion constructor/destructor

        #region node name and infos

        //provide node infos 
        private static IPluginInfo FPluginInfo;
        public static IPluginInfo PluginInfo
        {
            get
            {
                if (FPluginInfo == null)
                {
                    //fill out nodes info
                    //see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
                    FPluginInfo = new PluginInfo();

                    //the nodes main name: use CamelCaps and no spaces
                    FPluginInfo.Name = "LorenzAttractor";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "Animation";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "";

                    //the nodes author: your sign
                    FPluginInfo.Author = "fibo";
                    //describe the nodes function
                    FPluginInfo.Help = "Implements the very first Chaos Theory toy";
                    //specify a comma separated list of tags that describe the node
                    FPluginInfo.Tags = "";

                    //give credits to thirdparty code used
                    FPluginInfo.Credits = "";
                    //any known problems?
                    FPluginInfo.Bugs = "";
                    //any known usage of the node that may cause troubles?
                    FPluginInfo.Warnings = "";

                    //leave below as is
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                    System.Diagnostics.StackFrame sf = st.GetFrame(0);
                    System.Reflection.MethodBase method = sf.GetMethod();
                    FPluginInfo.Namespace = method.DeclaringType.Namespace;
                    FPluginInfo.Class = method.DeclaringType.Name;
                    //leave above as is
                }
                return FPluginInfo;
            }
        }

        public bool AutoEvaluate
        {
            //return true if this node needs to calculate every frame even if nobody asks for its output
            get { return false; }
        }

        #endregion node name and infos

        #region pin creation

        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            FHost = Host;

            //create inputs
            FHost.CreateValueInput("Input X", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FValueInputX);
            FValueInputX.SetSubType(double.MinValue, double.MaxValue, 0.01, 1, false, false, false);

            FHost.CreateValueInput("Input Y", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FValueInputY);
            FValueInputX.SetSubType(double.MinValue, double.MaxValue, 0.01, 2, false, false, false);

            FHost.CreateValueInput("Input Z", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FValueInputZ);
            FValueInputY.SetSubType(double.MinValue, double.MaxValue, 0.01, 3, false, false, false);

            FHost.CreateValueInput("deltaT", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FValueInputDeltaT);
            FValueInputDeltaT.SetSubType(0, 1, 0.001, 0.01, false, false, false);

            FHost.CreateValueInput("rho", 1, null, TSliceMode.Single, TPinVisibility.True, out FValueInputRho);
            FValueInputRho.SetSubType(double.MinValue, double.MaxValue, 0.01, 28, false, false, false);

            FHost.CreateValueInput("sigma", 1, null, TSliceMode.Single, TPinVisibility.True, out FValueInputSigma);
            FValueInputSigma.SetSubType(double.MinValue, double.MaxValue, 0.01, 10, false, false, false);

            FHost.CreateValueInput("beta", 1, null, TSliceMode.Single, TPinVisibility.True, out FValueInputBeta);
            FValueInputBeta.SetSubType(double.MinValue, double.MaxValue, 0.01, 2.667, false, false, false);

            //create outputs
            FHost.CreateValueOutput("Output X", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FValueOutputX);
            FValueOutputX.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            FHost.CreateValueOutput("Output Y", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FValueOutputY);
            FValueOutputY.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            FHost.CreateValueOutput("Output Z", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FValueOutputZ);
            FValueOutputZ.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
        }

        #endregion pin creation

        #region mainloop

        public void Configurate(IPluginConfig Input)
        {
            //nothing to configure in this plugin
            //only used in conjunction with inputs of type cmpdConfigurate
        }

        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int SpreadMax)
        {

            //the variables to fill with the input data

            // speed vector
            double currentValueSliceV1;
            double currentValueSliceV2;
            double currentValueSliceV3;

            if (FValueInputDeltaT.PinIsChanged)
                FValueInputDeltaT.GetValue(0, out deltaT);

            if (FValueInputRho.PinIsChanged)
                FValueInputRho.GetValue(0, out rho);

            if (FValueInputBeta.PinIsChanged)
                FValueInputBeta.GetValue(0, out beta);

            if (FValueInputSigma.PinIsChanged)
                FValueInputSigma.GetValue(0, out sigma);

            //if any of the inputs has changed
            //recompute the outputs
            if (FValueInputX.PinIsChanged || FValueInputY.PinIsChanged || FValueInputZ.PinIsChanged)
            {
                //first set slicecounts for all outputs
                //the incoming int SpreadMax is the maximum slicecount of all input pins, which is a good default
                FValueOutputX.SliceCount = SpreadMax;
                FValueOutputY.SliceCount = SpreadMax;
                FValueOutputZ.SliceCount = SpreadMax;

                previousValueSliceX = new double[SpreadMax];
                previousValueSliceY = new double[SpreadMax];
                previousValueSliceZ = new double[SpreadMax];

                for (int i = 0; i < SpreadMax; i++)
                {
                    //read data from inputs
                    FValueInputX.GetValue(i, out previousValueSliceX[i]);
                    FValueInputY.GetValue(i, out previousValueSliceY[i]);
                    FValueInputZ.GetValue(i, out previousValueSliceZ[i]);
                }

            }

            //loop for all slices
            for (int i = 0; i < SpreadMax; i++)
            {
                //compute Lorentz attractor equation
                currentValueSliceV1 = sigma * (previousValueSliceY[i] - previousValueSliceX[i]);
                currentValueSliceV2 = previousValueSliceX[i] * (rho - previousValueSliceZ[i]) - previousValueSliceY[i];
                currentValueSliceV3 = previousValueSliceX[i] * previousValueSliceY[i] - beta * previousValueSliceZ[i];

                currentValueSliceV1 *= deltaT;
                currentValueSliceV2 *= deltaT;
                currentValueSliceV3 *= deltaT;


                //update values
                previousValueSliceX[i] = previousValueSliceX[i] + currentValueSliceV1;
                previousValueSliceY[i] = previousValueSliceY[i] + currentValueSliceV2;
                previousValueSliceZ[i] = previousValueSliceZ[i] + currentValueSliceV3;

                //write data to outputs
                FValueOutputX.SetValue(i, previousValueSliceX[i]);
                FValueOutputY.SetValue(i, previousValueSliceY[i]);
                FValueOutputZ.SetValue(i, previousValueSliceZ[i]);
            }
        }

        #endregion mainloop
    }
}
