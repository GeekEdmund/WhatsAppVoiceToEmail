# WhatsApp Voice to Email

WhatsApp Voice to Email is an AI-driven system to convert WhatsApp voice notes into professional emails. By leveraging third-party services like Twilio, AssemblyAI, OpenAI, and SendGrid, this solution transcribes incoming voice messages, refines their content, and dispatches them as emails.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Prerequisites](#prerequisites)
- [Installation and Setup](#installation-and-setup)
- [Project Structure](#project-structure)
- [How It Works](#how-it-works)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

## Overview

In today's fast-paced world, integrating voice messaging with email communication can enhance customer engagement and streamline workflows. This project shows you how to:

- Receive voice notes via WhatsApp (using Twilio)
- Transcribe the voice message with AssemblyAI
- Enhance the transcribed text using OpenAI
- Send the enhanced content as an email using SendGrid

## Features

- **Voice Transcription:** Convert audio files into text using AssemblyAI.
- **Content Enhancement:** Refine and format the transcribed text into a professional email using OpenAI.
- **Email Dispatch:** Send personalized emails via SendGrid.
- **WhatsApp Integration:** Receive voice messages through Twilio webhooks.

## Prerequisites

Before you begin, ensure you have the following:

- [.NET 7.0 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- Visual Studio Code or Visual Studio 2022
- Git
- Accounts and API keys for:
  - [Twilio](https://www.twilio.com/)
  - [SendGrid](https://sendgrid.com/)
  - [OpenAI](https://openai.com/)
  - [AssemblyAI](https://www.assemblyai.com/)
  - [Ngrok](https://ngrok.com/) (for exposing local endpoints during testing)

## Installation and Setup

### 1. Clone the Repository

Clone this repository to your local machine:

```bash
git clone https://github.com/geekedmund/WhatsAppVoiceToEmail.git
```

### 2. Set Up the .NET Solution

Navigate to the repository folder:

```bash
cd WhatsAppVoiceToEmail
```

This solution contains two main projects:
- **VoiceToEmail.API:** Contains the API controllers and service implementations.
- **VoiceToEmail.Core:** Contains core models and interfaces.

### 3. Install Dependencies

For the API project, navigate into the project folder and install the necessary NuGet packages:

```bash
cd VoiceToEmail.API
dotnet add package OpenAI
dotnet add package SendGrid
dotnet add package AssemblyAI
dotnet add package Microsoft.EntityFrameworkCore.SQLite
```

### 4. Configure API Keys

Update the `appsettings.json` file in the **VoiceToEmail.API** project with your credentials:

```json
{
  "OpenAI": {
    "ApiKey": "YOUR_OPENAI_API_KEY"
  },
  "SendGrid": {
    "ApiKey": "YOUR_SENDGRID_API_KEY",
    "FromEmail": "YOUR_SENDGRID_FROM_EMAIL",
    "FromName": "YOUR_SENDGRID_FROM_NAME"
  },
  "Twilio": {
    "AccountSid": "YOUR_TWILIO_ACCOUNT_SID",
    "AuthToken": "YOUR_TWILIO_AUTH_TOKEN",
    "WhatsAppNumber": "YOUR_TWILIO_WHATSAPP_NUMBER"
  },
  "AssemblyAI": {
    "ApiKey": "YOUR_ASSEMBLYAI_API_KEY"
  },
  "AllowedHosts": "*"
}
```

### 5. Build and Run the Application

Build and run the API project using the following commands:

```bash
cd VoiceToEmail.API
dotnet build
dotnet run
```

### 6. Expose Your Local Endpoint

If you are testing the WhatsApp webhook locally, use Ngrok to expose your local endpoint:

```bash
ngrok http <PORT>
```

Replace `<PORT>` with the port your API is running on (e.g., 5168).

## Project Structure

```
WhatsAppVoiceToEmail/
├── VoiceToEmail.Core/          # Contains core models and interfaces
│   ├── Models/                # Domain models (e.g., VoiceMessage)
│   └── Interfaces/            # Service contracts (transcription, content enhancement, email service, etc.)
└── VoiceToEmail.API/           # Contains the Web API project
    ├── Controllers/           # API endpoints for message processing and webhooks
    ├── Services/              # Implementation of services
    ├── appsettings.json       # Configuration (API keys, etc.)
    └── Program.cs             # Application entry point
```

## How It Works

1. **Receive a Voice Note:**  
   A WhatsApp voice message is received through a Twilio webhook.

2. **Transcription:**  
   The audio file is uploaded to AssemblyAI, which transcribes it into text.

3. **Content Enhancement:**  
   The transcribed text is sent to OpenAI to generate a more polished, professional email version.

4. **Email Dispatch:**  
   The enhanced email content is sent via SendGrid to the intended recipient.

## Contributing

Contributions, issues, and feature requests are welcome!  
Feel free to check the [issues page](https://github.com/geekedmund/WhatsAppVoiceToEmail/issues) to see what needs help or to propose new ideas.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contact

For questions or support, please email [jacobsnipes254@gmail.com](mailto:jacobsnipes254@gmail.com).
