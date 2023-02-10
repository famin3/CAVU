# CAVU

# Notes
- Updating booking - assuming that it was initially paid for, we can't just update a price - we need to check for old price and new price and if these are different either ask for additional payment or inform about refund process
- Currently there's no checks for multiple prices per same date - there's not enough information to understand how that would be managed, so to simplify the solution I assumed it's not changing often and is managed by a competent person that does not create overlapping dates for prices
- No unique ID is implemented to make it simpler to add/remove anything for demo purposes
