const JournalEntry = require('../models/Journal'); // Made changes here from journal to journalEntry

// Create a new journal entry
const createEntry = async (req, res) => {
  try {
    const { title, content, date } = req.body;
    const newEntry = await JournalEntry.create({ ///made changes here
      title, 
      content, 
      date, 
      UserId: req.user.id 
    });
    res.status(201).json(newEntry);
  } catch (error) {
    res.status(500).json({ message: 'Failed to create entry' });
  }
};

// Get all entries for the authenticated user
const getEntries = async (req, res) => {
  try {
    const entries = await JournalEntry.findAll({ where: { UserId: req.user.id } });
    res.json(entries);
  } catch (error) {
    res.status(500).json({ message: 'Failed to fetch entries' });
  }
};

// Update an entry by ID
const updateEntry = async (req, res) => {
  try {
    const { id } = req.params;
    const { title, content, date } = req.body;

    const entry = await JournalEntry.findOne({ where: { id, UserId: req.user.id } });

    if (!entry) {
      return res.status(404).json({ message: 'Entry not found' });
    }

    await entry.update({ title, content, date });
    res.json(entry);
  } catch (error) {
    res.status(500).json({ message: 'Failed to update entry' });
  }
};

// Delete an entry
const deleteEntry = async (req, res) => {
  try {
    const { id } = req.params;

    const entry = await JournalEntry.findOne({ where: { id, UserId: req.user.id } });

    if (!entry) {
      return res.status(404).json({ message: 'Entry not found' });
    }

    await entry.destroy();
    res.json({ message: 'Entry deleted successfully' });
  } catch (error) {
    res.status(500).json({ message: 'Failed to delete entry' });
  }
};

// Toggle the starred status
const starEntry = async (req, res) => {
  try {
    const { id } = req.params;

    const entry = await JournalEntry.findOne({ where: { id, UserId: req.user.id } });

    if (!entry) {
      return res.status(404).json({ message: 'Entry not found' });
    }

    entry.isStarred = !entry.isStarred;
    await entry.save();
    res.json(entry);
  } catch (error) {
    res.status(500).json({ message: 'Failed to star/unstar entry' });
  }
};

// Export all functions
module.exports = {
  createEntry,
  getEntries,
  updateEntry,
  deleteEntry,
  starEntry
};
