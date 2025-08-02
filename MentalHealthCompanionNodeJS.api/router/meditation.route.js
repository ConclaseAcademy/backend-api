


  const express = require('express');
const router = express.Router();
const meditationService = require('../services/meditation.service');


router.get('/', async (req, res) => {
  try {
    const meditations = await meditationService.getFilteredMeditations(req.query);
    res.json(meditations);
  } catch (err) {
    res.status(500).json({ error: 'Failed to fetch meditations' });
  }
});

//
router.post('/seed', async (req, res) => {
  try {
    const meditations = await meditationService.seedMeditations(req.body);
    res.status(201).json(meditations);
  } catch (err) {
    res.status(500).json({ error: 'Failed to seed meditations' });
  }
});

module.exports = router