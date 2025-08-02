const { DataTypes } = require('sequelize');
const sequelize = require('../sequelize');
const User = require('./user');

const MoodEntry = sequelize.define('MoodEntry', {
    id: { type: DataTypes.UUID, defaultValue: DataTypes.UUIDV4, primaryKey: true },
    date: { type: DataTypes.DATEONLY, allowNull: false }, 
    mood: { type: DataTypes.STRING },
    emotions: { type: DataTypes.ARRAY(DataTypes.STRING) },
    triggers: { type: DataTypes.ARRAY(DataTypes.STRING) },
    wordCount: { type: DataTypes.INTEGER }
});

User.hasMany(MoodEntry);
MoodEntry.belongsTo(User);

module.exports = MoodEntry;
