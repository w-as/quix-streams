﻿using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuixStreams.Telemetry.Models;
using QuixStreams.Transport.Fw;
using QuixStreams.Transport.Kafka;

namespace QuixStreams.Telemetry.Kafka
{
    /// <summary>
    /// Kafka producer component implementation.
    /// It produces all the incoming messages to Kafka with a new StreamId.
    /// </summary>
    public class TelemetryKafkaProducer : StreamComponent, IDisposable
    {
        private readonly ILogger logger = Logging.CreateLogger<TelemetryKafkaProducer>();

        private QuixStreams.Transport.IO.IProducer transportProducer;

        /// <summary>
        /// The globally unique identifier of the stream.
        /// </summary>
        public string StreamId { get; protected set; }

        /// <summary>
        /// Event raised when an exception occurs during the publishing process
        /// </summary>
        public event EventHandler<Exception> OnWriteException;

        /// <summary>
        /// Initializes a new instance of <see cref="TelemetryKafkaProducer"/>
        /// </summary>
        /// <param name="producer">A stream package producer. Share this among multiple instances of this class to prevent re-initialization.</param>
        /// <param name="byteSplitter">The byte splitter to use. </param>
        /// <param name="streamId">Stream Id to use to generate the new Stream on Kafka. If not specified, it generates a new Guid.</param>
        public TelemetryKafkaProducer(QuixStreams.Transport.IO.IProducer producer, IByteSplitter byteSplitter, string streamId = null)
        {
            this.transportProducer = new QuixStreams.Transport.TransportProducer(producer, byteSplitter);

            this.InitializeStreaming(streamId);
        }

        private void InitializeStreaming(string streamId)
        {
            this.StreamId = streamId ?? Guid.NewGuid().ToString();
            this.Input.Subscribe(OnStreamPackage);
        }

        private Task OnStreamPackage(StreamPackage package)
        {
            return this.SendAsync(package);
        }

        private async Task SendAsync(StreamPackage package)
        {
            try
            {
                var transportPackage = new QuixStreams.Transport.IO.Package(
                    package.Type,
                    package.Value
                );

                if (transportProducer == null)
                {
                    throw new InvalidOperationException("Writer is already closed.");
                }

                transportPackage.SetKey(Encoding.UTF8.GetBytes(StreamId));

                await this.transportProducer.Publish(transportPackage, this.CancellationToken);
                await this.Output.Send(package);
            }
            catch (Exception e)
            {
                if (this.OnWriteException == null)
                {
                    this.logger.LogError(e, "Exception sending package to Kafka");
                }
                else
                {
                    this.OnWriteException?.Invoke(this, e);
                }
            }
        }

        private void Stop()
        {
            // Transport layer
            this.transportProducer = null;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Stop();
        }

    }
}
