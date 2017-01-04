﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using LSL;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Assets.LSL4Unity.Scripts
{
    /// <summary>
    /// Encapsulates the lookup logic for LSL streams with an event based appraoch
    /// your custom stream inlet implementations could be subscribed to the On
    /// </summary>
    public class Resolver : MonoBehaviour, IEventSystemHandler {

        public List<LSLStreamInfoWrapper> knownStreams;

        public float forgetStreamAfter = 1.0f;

        private liblsl.ContinuousResolver resolver;

        private bool resolve = true;

        public StreamEvent onStreamFound = new StreamEvent();

        public StreamEvent onStreamLost = new StreamEvent();

		public bool IsStreamAvailable(String StreamName, String StreamType, out LSLStreamInfoWrapper streamInfo)
		{
			bool skipName = StreamName.Length == 0;
			bool skipType = StreamType.Length == 0;
			foreach (LSLStreamInfoWrapper infowrap in knownStreams) {
				bool nameOk = skipName || infowrap.Name.Equals(StreamName);
				bool typeOk = skipType || infowrap.Type.Equals (StreamType);
				if (nameOk && typeOk) {
					streamInfo = infowrap;
					return true;
				}
			}
			streamInfo = null;
			return false;
		}
        
        // Use this for initialization
        void Start() {

            resolver = new liblsl.ContinuousResolver(forgetStreamAfter);

            StartCoroutine(resolveContinuously());
        }

        private IEnumerator resolveContinuously()
        {
            while (resolve)
            {
                var results = resolver.results();
                
                //If any stream in knownStreams is no longer in results,
                //trigger onStreamLost and remove it from cache.
                foreach (var item in knownStreams)
                {
                    if (!results.Any(r => r.name().Equals(item.Name)))
                    {
                        if (onStreamLost.GetPersistentEventCount() > 0)
                            onStreamLost.Invoke(item);
                    }
                }
                knownStreams.RemoveAll(s => !results.Any(r => r.name().Equals(s.Name)));

                //TODO: if any new objects are recently registered,
                // we need to trigger their onStreamFound events with old streams.

                // add new found streams to the cache and trigger listener onStreamFound events.
                foreach (var item in results)
                {
                    if(!knownStreams.Any(s => s.Name == item.name())){

                        Debug.Log(string.Format("Found new Stream {0}", item.name()));

                        var newStreamInfo = new LSLStreamInfoWrapper(item);
                        knownStreams.Add(newStreamInfo);

                        onStreamFound.Invoke(newStreamInfo);
                    }
                }

                yield return new WaitForSecondsRealtime(0.1f);
            }
            yield return null;
        }
    }  

    [Serializable]
    public class LSLStreamInfoWrapper
    {
        public string Name;

        public string Type;
        
        private liblsl.StreamInfo item;
        private readonly string streamUID;

        private readonly int channelCount;
        private readonly string sessionId;
        private readonly string sourceID;
        private readonly double dataRate;
        private readonly string hostName;
        private readonly int streamVersion;

        public LSLStreamInfoWrapper(liblsl.StreamInfo item)
        {
            this.item = item;
            Name = item.name();
            Type = item.type();
            channelCount = item.channel_count();
            streamUID = item.uid();
            sessionId = item.session_id();
            sourceID = item.source_id();
            dataRate = item.nominal_srate();
            hostName = item.hostname();
            streamVersion = item.version();
        }

        public liblsl.StreamInfo Item
        {
            get
            {
                return item;
            }
        }

        public string StreamUID
        {
            get
            {
                return streamUID;
            }
        }

        public int ChannelCount
        {
            get
            {
                return channelCount;
            }
        }

        public string SessionId
        {
            get
            {
                return sessionId;
            }
        }

        public string SourceID
        {
            get
            {
                return sourceID;
            }
        }

        public string HostName
        {
            get
            {
                return hostName;
            }
        }

        public double DataRate
        {
            get
            {
                return dataRate;
            }
        }

        public int StreamVersion
        {
            get
            {
                return streamVersion;
            }
        }
    }

    [Serializable]
    public class StreamEvent : UnityEvent<LSLStreamInfoWrapper>{}
}

