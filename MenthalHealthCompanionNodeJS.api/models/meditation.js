const {DataTypes} =require('sequelize')
const sequelize = require('../sequelize')

const Meditation =sequelize.define('Meditation', {
    id: { type: DataTypes.UUID, defaultValue: DataTypes.UUIDV4, primaryKey: true },
    title: {type: DataTypes.STRING, alloNull: false},
    description: {type:DataTypes.TEXT},
    category: {type:DataTypes.STRING},
    duration: {type: DataTypes.INTEGER},
    author: {type:DataTypes.STRING},
    audioUrl: {type:DataTypes.STRING},
},{
    timestamps: true
});

module.exports =Meditation;

