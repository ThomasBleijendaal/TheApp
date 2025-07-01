# The App
It's the app

## Service Bus

### Design

Several principles:

- Moving parts all consist of interfaces that abstract away any packages

- Service bus message source is behind IAsyncEnumerable<Message>
-- This source must be ok with pauses in reading the messages - There might be back pressure that stalls reading the messages a bit.
-- This source must keep messages that have been dequeued valid until the pipeline completes / dlq's / retries the message.

- Service bus message sink is behind IAsyncProcessor
-- Sink must return MessageResult, a result object that indicates what should happen with the message

- Service bus message handling is behind IMessageHandler
-- This exposes the common message actions - complete, DLQ, retry, and renewLock

- Message processor is based on TPL
-- Rate limiting is based on back pressure
-- Max queue size is configurable
-- 
