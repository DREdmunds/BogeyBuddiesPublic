# Golf League Scores and Messages

This JSON structure represents the weekly scores and messages for our golf league:

```json
{
  "id": "week-1",
  "type": "weekly-scores",
  "weekIdentifier": "Week 1",
  "date": "2023-11-09",
  "scores": [
    {
      "playerId": "Derek",
      "grossScore": 82,
      "handicap": 9,
      "netScore": 73,
      "weeklyWinnings": 30,
      "skinsWon": [
        {
          "hole": 5,
          "amount": 20
        },
        {
          "hole": 16,
          "amount": 20
        }
      ]
    }
    // ... additional players
  ],
  "messages": [
    {
      "messageId": "message1",
      "author": "Player1",
      "content": "Great game today!",
      "timestamp": "2023-07-01T18:30:00Z"
    }
    // ... additional messages
  ]
}
