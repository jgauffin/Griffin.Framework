# Reliable

These implementations of ICommandBus and IEventBus will store all enqueued items 
in a queue before invoking them. 

This means that all items that have been
enqueued will be invoked eventually. It might be directly or
as soon as the application starts if it crashes.

We recommend that you use the `OneFilePerItemQueue' or similar.

As these implementations uses tasks in the background you can also enable multiple 
concurrent workers to speed up the process.
