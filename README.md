# MessageQueueApp

## Project Overview
MessageQueueApp is a .NET 6 console application solution demonstrating a producer-consumer messaging system using Microsoft Message Queuing (MSMQ). The project implements a reliable mechanism for producing and consuming appointment messages, with support for dead letter queue handling, configurable retry policies, and centralized logging.

## Features
- Producer application that generates and sends serialized appointment messages to MSMQ.
- Consumer application that listens and processes messages from MSMQ.
- Dead letter queue support for handling failed messages.
- Configurable settings per application via `appsettings.json`.
- Centralized logging library indicating project source and log level.
- Clean architecture leveraging dependency injection.
- Separate Message processing repository in `MessageQueueApp.Messaging` class library.

## Architecture & Design
- Multiple projects:
  - `MessageQueueApp.Producer`: Generates and sends messages to MSMQ.
  - `MessageQueueApp.Consumer`: Receives and processes messages from MSMQ.
  - `MessageQueueApp.Common`: Holds shared models and utilities.
  - `MessageQueueApp.Logging`: Provides thread-safe console logging.
  - `MessageQueueApp.Messaging`: Contains MSMQ client abstraction and implementation.
- JSON serialization/deserialization of messages for communication.
- Dependency injection used to manage dependencies and promote testability.
- Configurations managed independently at each project level.

## Prerequisites
- .NET 6.0 SDK installed.
- Windows OS with Microsoft Message Queuing (MSMQ) feature enabled.
- Visual Studio 2022 or later recommended.
- MSMQ private queues will be created automatically if not present.

## Getting Started
1. Clone the repository:  
   ```bash
   git clone <repo-url>
2. **Open the solution:**
   
   Open `MessageQueueApp.sln` in Visual Studio 2022 or later.

3. **Restore packages and build:**

   Restore NuGet packages automatically or run:

   ```bash
   dotnet restore
   dotnet build
   ```

4. **Configure MSMQ (if needed):**

   Ensure MSMQ feature is enabled on your Windows machine.

5. **Set startup projects:**

   - In Visual Studio, right-click on Solution -> Properties -> Startup Project.
   - Select “Multiple startup projects”.
   - Set both `MessageQueueApp.Producer` and `MessageQueueApp.Consumer` action to “Start”.

6. **Configure appsettings.json:**

   Each console project has an `appsettings.json` you can customize.

---

## Configuration

Below are typical settings configurable in each app's `appsettings.json` file:

### Producer appsettings.json example

```json
{
  "ProjectName": "MessageQueueApp.Producer",
  "QueuePath": ".\\Private$\\AppointmentQueue",
  "DeadLetterQueuePath": ".\\Private$\\AppointmentDeadLetter", // Added for clearing dead letter queue 
  "MaxMessageCount": 100,
  "MaxFailedMessages": 10,
  "MessageDelayMilliseconds": 50
}
```

### Consumer appsettings.json example

```json
{
  "ProjectName": "MessageQueueApp.Consumer",
  "QueuePath": ".\\Private$\\AppointmentQueue",
  "DeadLetterQueuePath": ".\\Private$\\AppointmentDeadLetter",
  "MaxRetryCount": 2,
  "CheckIntervalSeconds": 10,
  "DeadLetterWarningThreshold": 5
}
```

- `ProjectName`: Used by the logging system to identify the source.
- `QueuePath`: MSMQ path for main message queue.
- `DeadLetterQueuePath`: MSMQ path for dead letter queue (optional for Producer).
- Other keys configure retry limits, delays, batch sizes, and intervals.

---

## Running the Applications

- Start both Producer and Consumer projects using Visual Studio’s multiple startup projects feature.
- **Producer behavior:**
  - Prompts the user to input how many messages to generate (up to `MaxMessageCount`).
  - Asks how many messages should be marked as failed (simulated).
  - Generates and sends messages to MSMQ accordingly.
  - Runs continuously until user types “exit”.
  - After all message processed if you want clear dead letter queue type "DQ".

- **Consumer behavior:**
  - Continuously polls the MSMQ for messages.
  - Processes messages and logs successes or failures.
  - Moves failed messages to dead letter queue after exceeding retry counts.
  - Displays warning when dead letter queue reaches specified threshold.

---

## Usage Example

**Producer Console:**

```
Press Enter to continue, or type exit to quit

Enter number of messages to generate (Max 100):
10
Enter number of messages to fail (0 to 10[dynamic], & Max 10 [configurable]):
3
Batch complete. Messages generated: 10, failed messages: 3
```

**Consumer Console:**

```
Consumer started listening to MSMQ.
Watchlist API Success for : Appointment has been created for Ticket: ABCD-06/15-10000 and container: WXYZ1234560
Watchlist API Failed for : Appointment has been created for Ticket: ABCD-06/15-10001 and container: WXYZ6543210
Warning: Dead Letter queue has reached 5 messages.
```

---

## Project Structure

```
MessageQueueApp/
├── MessageQueueApp.Common/          # Shared models and utilities
├── MessageQueueApp.Logging/         # Logging library
├── MessageQueueApp.Messaging/       # MSMQ client abstraction and implementation
├── MessageQueueApp.Producer/        # Producer console app
├── MessageQueueApp.Consumer/        # Consumer console app
├── README.md
```

---

## Important Code Highlights

- Strongly typed message serialization with JSON.
- Centralized thread-safe logging with dynamic project identification.
- Dead letter queue management in case of message failures.
- Dependency Injection configured for extensibility and testing.
- Message processing repository (`MessageQueueApp.Messaging`) cleanly separates MSMQ details.

---

## Limitations and Known Issues

- MSMQ functionality limited to Windows OS.
- No transactional message receive/commit implemented yet.
- Future enhancement needed for unit and integration tests.
- No UI dashboard; strictly console based.

---

## Future Enhancements

- Implement transactional message processing.
- Support for alternative message queues (RabbitMQ, Azure Service Bus).
- Add monitoring and UI dashboard.
- Containerize applications for easier deployment.

---

## Contributing

Contributions, issues, and feature requests are welcome! Please open an issue or submit a pull request.

---

## Contact

For questions or feedback, please contact Nikunj Katariya.
