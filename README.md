# Jaju Short Skirt

A lightweight and easy-to-use Discord bot built with C# and Discord.NET.
Designed for easy customization and learning — perfect for beginners experimenting with Discord.NET.
( the name is an inside joke )

---

## 🚀 Features

- **Modular command handling**
- **A simble economy system based on working/gambling🤑 and global leaderboards**
- **Basic API consumption (e.g., Spotify)**
- **Custom AI command using a local DeepSeek model**
- **Beginner-friendly structure**

---

## 🛠️ Setup Instructions

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download)
- [Discord Bot Token](https://discord.com/developers/applications)

### Optional Prerequisites (tho bot wont turn on without them 🔜)
- Spotify API credentials 
- Locally hosted DeepSeek AI endpoint
- Locally hosted MySql (or any dbms u want tho u will have to do some shenanigans)

### 1. Clone the Repository

```bash
git clone https://github.com/Gorgoll/JajuShortSkirt.git
cd JajuShortSkirt

```
### 2. Configure Environment

```
{
  "Token": "YOUR_DISCORD_BOT_TOKEN",
  "SpotifyClientId": "YOUR_CLIENT_ID",
  "SpotifyClientSecret": "YOUR_CLIENT_SECRET",
  "DeepSeekEndpoint": "http://localhost:5000/api/inference"
}
```

### 3. Run the Bot
```
bash dotnet run
```


### 🧠 DeepSeek AI Integration
Make sure your DeepSeek AI model is locally hosted and accessible at the endpoint specified in your config. The bot sends user prompts and returns AI-generated responses.
(the default model this bot uses is 8b)

### 🤝 Contributing
Pull requests are welcome!

### 📄 License
This project is open-source and available under the MIT License.

