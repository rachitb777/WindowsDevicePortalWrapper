﻿//----------------------------------------------------------------------------------------------
// <copyright file="PerceptionSimulationRecording.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for Perception Simulation Recording methods
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for getting a Holographic Perception Simulation recording status.
        /// </summary>
        public static readonly string HolographicSimulationRecordingStatusApi = "api/holographic/simulation/recording/status";

        /// <summary>
        /// API for starting a Holographic Perception Simulation recording.
        /// </summary>
        public static readonly string StartHolographicSimulationRecordingApi = "api/holographic/simulation/recording/start";

        /// <summary>
        /// API for stopping a Holographic Perception Simulation recording.
        /// </summary>
        public static readonly string StopHolographicSimulationRecordingApi = "api/holographic/simulation/recording/stop";

        /// <summary>
        /// Gets the holographic simulation recording status.
        /// </summary>
        /// <returns>True if recording, false otherwise.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task<bool> GetHolographicSimulationRecordingStatus()
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            HolographicSimulationRecordingStatus status = await this.Get<HolographicSimulationRecordingStatus>(HolographicSimulationRecordingStatusApi);
            return status.IsRecording;
        }

        /// <summary>
        /// Starts a Holographic Simulation recording session.
        /// </summary>
        /// <param name="name">The name of the recording.</param>
        /// <param name="recordHead">Should head data be recorded? The default value is true.</param>
        /// <param name="recordHands">Should hand data be recorded? The default value is true.</param>
        /// <param name="recordSpatialMapping">Should Spatial Mapping data be recorded? The default value is true.</param>
        /// <param name="recordEnvironment">Should environment data be recorded? The default value is true.</param>
        /// <param name="singleSpatialMappingFrame">Should the spatial mapping data be limited to a single frame? The default value is false.</param>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task StartHolographicSimulationRecording(
            string name,
            bool recordHead = true,
            bool recordHands = true,
            bool recordSpatialMapping = true,
            bool recordEnvironment = true)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            string payload = string.Format(
                "head={0}&hands={1}&spatialMapping={2}&environment={3}&name={4}",
                recordHead ? 1 : 0,
                recordHands ? 1 : 0,
                recordSpatialMapping ? 1 : 0,
                recordEnvironment ? 1: 0,
                name);
            await this.Post(StartHolographicSimulationRecordingApi, payload);
        }

        /// <summary>
        /// Stops a Holographic Simulation recording session.
        /// </summary>
        /// <returns>Byte array containing the recorded data.</returns>
        /// <exception cref="InvalidOperationException">No recording was in progress.</exception>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task<byte[]> StopHolographicSimulationRecording()
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                StopHolographicSimulationRecordingApi);

            byte[] dataBytes = null;

            using (Stream dataStream = await this.Get(uri))
            {
                if ((dataStream != null) &&
                    (dataStream.Length != 0))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(HolographicSimulationStopRecordingError));
                    HolographicSimulationStopRecordingError error = null;
 
                    try
                    {
                        // Try to get / interpret an error response.
                        error = (HolographicSimulationStopRecordingError)serializer.ReadObject(dataStream);
                    }
                    catch
                    {
                    }

                    if (error != null)
                    {
                        // We received an error response.
                        throw new InvalidOperationException(error.Reason);
                    }

                    // Getting here indicates that we have file data to return.
                    dataBytes = new byte[dataStream.Length];
                    dataStream.Read(dataBytes, 0, dataBytes.Length);
                }
            }

            return dataBytes;
        }

        #region Data contract
        public class HolographicSimulationStopRecordingError
        {
            [DataMember(Name = "Reason")]
            public string Reason { get; set; }
        }


        /// <summary>
        /// Object representation of Holographic Simulation recording status.
        /// </summary>
        [DataContract]
        public class HolographicSimulationRecordingStatus
        {
            /// <summary>
            /// Gets or sets the recording status.
            /// </summary>
            [DataMember(Name = "recording")]
            public bool IsRecording { get; set; }
        }
        #endregion // Data contract
    }
}
