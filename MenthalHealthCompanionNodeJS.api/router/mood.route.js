const express = require('express');
const router = express.Router();
const moodService = require('../services/mood.service');
const {authenticate} = require('../passportJwtValidation');

router.post('/track', authenticate, moodService.trackMood);
router.get('/today', authenticate, moodService.getTodayEntry);
router.get('/week', authenticate, moodService.getWeeklyEntries);

module.exports = router;
