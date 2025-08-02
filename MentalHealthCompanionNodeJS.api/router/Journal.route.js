const express = require('express');
const router = express.Router();

const journalService = require('../services/Journal.service');
console.log(journalService)
const { createEntry, getEntries, updateEntry, deleteEntry, starEntry } = journalService;
const {authenticate} = require('../passportJwtValidation')




router.post('/new', authenticate, createEntry);
router.get('/', authenticate, getEntries);
router.put('/:id', authenticate, updateEntry);
router.delete('/:id', authenticate, deleteEntry);
router.patch('/:id/star', authenticate, starEntry);


  
module.exports = router;
