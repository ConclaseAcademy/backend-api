const { DataTypes } = require('sequelize');
const sequelize = require('../sequelize');
const User = require('./user');

const JournalEntry = sequelize.define('JournalEntry', {
  id: {
    type: DataTypes.UUID,
    defaultValue: DataTypes.UUIDV4,
    primaryKey: true
  },
  title: DataTypes.STRING,
  content: DataTypes.TEXT,
  date: DataTypes.DATEONLY,
  isStarred: {
    type: DataTypes.BOOLEAN,
    defaultValue: false
  }
});

User.hasMany(JournalEntry);
JournalEntry.belongsTo(User);

module.exports = JournalEntry;
