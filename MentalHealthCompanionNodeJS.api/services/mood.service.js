const { Op } = require('sequelize');
const MoodEntry = require('../models/moodEntry');

exports.trackMood = async (req, res) => {
  const { mood, emotions, triggers, wordCount } = req.body;
  const date = new Date().toISOString().split('T')[0];

  try {
    const entry = await MoodEntry.upsert({
      UserId: req.user.id,
      date,
      mood,
      emotions,
      triggers,
      wordCount
    });
    res.status(200).json({ message: 'Mood tracked successfully', entry });
  } catch (err) {
    res.status(500).json({ message: 'Error tracking mood', error: err.message });
  }
};

exports.getTodayEntry = async (req, res) => {
  const today = new Date().toISOString().split('T')[0];
  try {
    const entry = await MoodEntry.findOne({
      where: {
        UserId: req.user.id,
        date: today
      }
    });

    if (!entry) return res.status(404).json({ message: "No mood entry for today" });

    res.json(entry);
  } catch (err) {
    res.status(500).json({ message: "Failed to fetch today's mood", error: err.message });
  }
};

exports.getWeeklyEntries = async (req, res) => {
  const today = new Date();
  const weekAgo = new Date();
  weekAgo.setDate(today.getDate() - 6); // Last 7 days including today

  try {
    const entries = await MoodEntry.findAll({
      where: {
        UserId: req.user.id,
        date: {
          [Op.between]: [weekAgo.toISOString().split('T')[0], today.toISOString().split('T')[0]]
        }
      },
      order: [['date', 'ASC']]
    });

    res.json(entries);
  } catch (err) {
    res.status(500).json({ message: "Failed to fetch weekly moods", error: err.message });
  }
};
