using System;
using System.Collections.Generic;
using DotNetCqs;
using Griffin.Cqs.Simple;

namespace Griffin.Cqs;

/// <summary>
///     Used by <see cref="SimpleMessageBus.HandlerFailed" />.
/// </summary>
public class MessageHandlerFailedEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MessageHandlerFailedEventArgs" /> class.
    /// </summary>
    /// <param name="message">The message that one or more handlers failed to process.</param>
    /// <param name="failures">One instance per handler.</param>
    /// <param name="handlerCount">Total amount of subscribers (and not just the amount of failed handlers).</param>
    /// <exception cref="System.ArgumentNullException">
    ///     applicationEvent
    ///     or
    ///     failures
    /// </exception>
    /// <exception cref="System.ArgumentOutOfRangeException">handlerCount;Suspicions handler count</exception>
    public MessageHandlerFailedEventArgs(Message message, IReadOnlyList<HandlerFailure> failures,
        int handlerCount)
    {
        if (handlerCount < 0 || handlerCount > 1000)
            throw new ArgumentOutOfRangeException("handlerCount", handlerCount, "Suspicions handler count");

        Message = message ?? throw new ArgumentNullException("message");
        Failures = failures ?? throw new ArgumentNullException("failures");
        HandlerCount = handlerCount;
    }

    /// <summary>
    ///     Handlers that failed to consume the event and why they failed
    /// </summary>
    public IReadOnlyList<HandlerFailure> Failures { get; }

    /// <summary>
    ///     Total amount of subscribers (and not just the amount of failed handlers)
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Can be used to determine if all or just some handlers failed.
    ///     </para>
    /// </remarks>
    public int HandlerCount { get; }

    /// <summary>
    ///     Event that some handlers failed to consume.
    /// </summary>
    public Message Message { get; }
}