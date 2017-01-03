using LSL;
using System;
using UnityEngine;

namespace Assets.LSL4Unity.Scripts.AbstractInlets
{
    public abstract class ABaseInlet : MonoBehaviour
    {
        public string StreamName;

        public string StreamType;

        protected liblsl.StreamInlet inlet;

        protected int expectedChannels;

        protected double[] timestamps;

		void Start()
		{
			var resolver = FindObjectOfType<Resolver>();

			LSLStreamInfoWrapper streamInfo;
			bool isAvailable = resolver.IsStreamAvailable(StreamName, StreamType, out streamInfo);

			if(isAvailable){
				// use it immediately
				this.AStreamIsFound(streamInfo);
			}else
			{
				// register listener dynamically for the event which is expected later....
				resolver.onStreamFound.AddListener(this.AStreamIsFound);
				resolver.onStreamLost.AddListener (this.AStreamGotLost);
			}

			StartImpl ();
		}

        /// <summary>
        /// Callback method for the Resolver gets called each time the resolver found a stream
        /// </summary>
        /// <param name="stream"></param>
        public virtual void AStreamIsFound(LSLStreamInfoWrapper stream)
        {
            if (!isTheExpected(stream))
                return;

            inlet = new LSL.liblsl.StreamInlet(stream.Item, 1, 1);
            expectedChannels = stream.ChannelCount;

            OnStreamAvailable();
        }

        /// <summary>
        /// Callback method for the Resolver gets called each time the resolver misses a stream within its cache
        /// </summary>
        /// <param name="stream"></param>
        public virtual void AStreamGotLost(LSLStreamInfoWrapper stream)
        {
            if (!isTheExpected(stream))
                return;

            Debug.Log(string.Format("LSL Stream {0} Lost for {1}", stream.Name, name));

            this.enabled = false;
        }

        private bool isTheExpected(LSLStreamInfoWrapper stream)
        {
            bool result = false;
            if (StreamName.Length > 0 && StreamType.Length > 0)
            {
                // If both StreamName and StreamType are provided then both must match.
                result |= (StreamName.Equals(stream.Name) && StreamType.Equals(stream.Type));
            }
            else if (StreamName.Length > 0)
            {
                result |= StreamName.Equals(stream.Name);
            }
            else if (StreamType.Length > 0)
            {
                result |= StreamType.Equals(stream.Type);
            }
            return result;
        }

		protected abstract void StartImpl();

        protected abstract void pullSamples();

        protected abstract void pullChunk();

        protected virtual void OnStreamAvailable()
        {
            // base implementation may not decide what happens when the stream gets available
            throw new NotImplementedException("Please override this method in a derived class!");
        }
    }

    public abstract class InletFloatSamples : ABaseInlet
    {
        protected abstract void Process(float[] newSample, double timeStamp);

        protected float[] sample;

        protected float[,] chunk;

        protected override void pullSamples()
        {
            sample = new float[expectedChannels];

            try
            {
                double lastTimeStamp;
                // pull as long samples are available
                while ((lastTimeStamp = inlet.pull_sample(sample, 0.0f)) != 0)
                {
                    Process(sample, lastTimeStamp);
                }
            }
            catch (ArgumentException aex)
            {
                Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
                this.enabled = false;
                Debug.LogException(aex, this);
            }

        }

        protected override void pullChunk()
        {
            int nSamples = inlet.samples_available();
            if (nSamples > 0)
            {
                chunk = new float[expectedChannels, nSamples];
                timestamps = new double[nSamples];
                int nSampReturned = inlet.pull_chunk(chunk, timestamps);
            }
            
        }
    }

    public abstract class InletDoubleSamples : ABaseInlet
    {
        protected abstract void Process(double[] newSample, double timeStamp);

        protected double[] sample;

        protected override void pullSamples()
        {
            sample = new double[expectedChannels];

            try
            {
                double lastTimeStamp;
                // pull as long samples are available
                while ((lastTimeStamp = inlet.pull_sample(sample, 0.0f)) != 0)
                {
                    Process(sample, lastTimeStamp);
                }
            }
            catch (ArgumentException aex)
            {
                Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
                this.enabled = false;
                Debug.LogException(aex, this);
            }

        }

        protected override void pullChunk()
        {

        }
    }

    public abstract class InletIntSamples : ABaseInlet
    {
        protected abstract void Process(int[] newSample, double timeStamp);

        protected int[] sample;

        protected override void pullSamples()
        {
            sample = new int[expectedChannels];

            try
            {
                double lastTimeStamp;
                // pull as long samples are available
                while ((lastTimeStamp = inlet.pull_sample(sample, 0.0f)) != 0)
                {
                    Process(sample, lastTimeStamp);
                }
            }
            catch (ArgumentException aex)
            {
                Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
                this.enabled = false;
                Debug.LogException(aex, this);
            }

        }

        protected override void pullChunk()
        {

        }
    }

    public abstract class InletCharSamples : ABaseInlet
    {
        protected abstract void Process(char[] newSample, double timeStamp);

        protected char[] sample;

        protected override void pullSamples()
        {
            sample = new char[expectedChannels];

            try
            {
                double lastTimeStamp;
                // pull as long samples are available
                while ((lastTimeStamp = inlet.pull_sample(sample, 0.0f)) != 0)
                {
                    Process(sample, lastTimeStamp);
                }
            }
            catch (ArgumentException aex)
            {
                Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
                this.enabled = false;
                Debug.LogException(aex, this);
            }

        }

        protected override void pullChunk()
        {

        }
    }

    public abstract class InletStringSamples : ABaseInlet
    {
        protected abstract void Process(String[] newSample, double timeStamp);

        protected String[] sample;

        protected override void pullSamples()
        {
            sample = new String[expectedChannels];

            try
            {
                double lastTimeStamp;
                // pull as long samples are available
                while ((lastTimeStamp = inlet.pull_sample(sample, 0.0f)) != 0)
                {
                    Process(sample, lastTimeStamp);
                }
            }
            catch (ArgumentException aex)
            {
                Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
                this.enabled = false;
                Debug.LogException(aex, this);
            }

        }

        protected override void pullChunk()
        {

        }
    }

    public abstract class InletShortSamples : ABaseInlet
    {
        protected abstract void Process(short[] newSample, double timeStamp);

        protected short[] sample;

        protected override void pullSamples()
        {
            sample = new short[expectedChannels];

            try
            {
                double lastTimeStamp;
                // pull as long samples are available
                while ((lastTimeStamp = inlet.pull_sample(sample, 0.0f)) != 0)
                {
                    Process(sample, lastTimeStamp);
                }
            }
            catch (ArgumentException aex)
            {
                Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
                this.enabled = false;
                Debug.LogException(aex, this);
            }

        }

        protected override void pullChunk()
        {

        }
    }


}
