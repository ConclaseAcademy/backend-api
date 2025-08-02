const { Op } = require('sequelize');
const Meditation = require('../models/meditation')

const getFilteredMeditations = async (filters) => {
  const { category, duration } = filters
  const where = {} //This is an empty box where we will write down the rules for what kind of meditations to look for

  if (category) where.category = category;

  if (duration) {
    const durationInt = parseInt(duration);
    if (durationInt === 5) {
      where.duration = { [Op.between]: [5, 10] };
    } else if (durationInt === 10) {
      where.duration = { [Op.between]: [10, 15] };
    } else if (durationInt === 15) {
      where.duration = { [Op.gt]: 15 };
    }
  }

  return Meditation.findAll({ where });
};

const seedMeditations = async (data) => {
  return Meditation.bulkCreate(data);
};

module.exports = {
  getFilteredMeditations,
  seedMeditations,
};